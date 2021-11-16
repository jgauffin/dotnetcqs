using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.Logging;
using DotNetCqs.MessageProcessor;
using Microsoft.Extensions.Logging;

namespace DotNetCqs.Queues
{
    /// <summary>
    ///     Listens on inbound inboundQueue to be able to process messages.
    /// </summary>
    public class QueueListener
    {
        private readonly IMessageRouter _outboundRouter;
        private readonly IMessageQueue _queue;
        private readonly IHandlerScopeFactory _scopeFactory;
        private TimeSpan[] _retryAttempts = new TimeSpan[0];
        private ILogger _logger = LogConfiguration.LogFactory.CreateLogger(typeof(QueueListener));

        /// <summary>
        ///     Creates a new instance of <see cref="QueueListener" />.
        /// </summary>
        /// <param name="inboundQueue">Used to receive messages</param>
        /// <param name="outboundQueue">All messages enqueued in handlers will be sent to this queue</param>
        /// <param name="scopeFactory">Used to create a new scope each time a message is handled.</param>
        public QueueListener(IMessageQueue inboundQueue, IMessageQueue outboundQueue, IHandlerScopeFactory scopeFactory)
        {
            _outboundRouter = new SingleQueueRouter(outboundQueue);
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _queue = inboundQueue ?? throw new ArgumentNullException(nameof(inboundQueue));
        }

        /// <summary>
        ///     Creates a new instance of <see cref="QueueListener" />.
        /// </summary>
        /// <param name="inboundQueue">Used to receive messages</param>
        /// <param name="outboundRouter">Routes messages through different queues</param>
        /// <param name="scopeFactory"></param>
        public QueueListener(IMessageQueue inboundQueue, IMessageRouter outboundRouter,
            IHandlerScopeFactory scopeFactory)
        {
            _outboundRouter = outboundRouter;

            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _queue = inboundQueue ?? throw new ArgumentNullException(nameof(inboundQueue));
        }

        /// <summary>
        /// Use a factory instead of the handler scope to create a message invoker.
        /// </summary>
        public Func<IHandlerScope, IMessageInvoker> MessageInvokerFactory { get; set; }


        /// <summary>
        ///     Intervals at which retries should be made for messages that can not be handled.
        /// </summary>
        public TimeSpan[] RetryAttempts
        {
            get => _retryAttempts;
            set => _retryAttempts = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Have attempted several times (<see cref="RetryAttempts" />) to handle a message and failed.
        /// </summary>
        public event EventHandler<PoisonMessageEventArgs> PoisonMessageDetected;

        public Task ProcessMessageAsync(ClaimsPrincipal principal, Message message)
        {
            return ProcessMessageAsync(new DequeuedMessage(principal, message));
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveSingleMessageAsync()
        {
            var wrapper = new MsgWrapper();
            using (var session = _queue.BeginSession())
            {
                await ReceiveSingleMessageAsync(wrapper, session);
                await session.SaveChanges();
            }
        }

        public async Task RunAsync(CancellationToken token)
        {
            var wrapper = new MsgWrapper();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    //only error here is DB related.
                    await HandleOneMessage(token, wrapper);
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                    {
                        break;
                    }

                    _logger.Error(_queue.Name, "Message handling failed.", ex);
                    await Task.Delay(1000, token);
                }

            }

            _logger.Info(_queue.Name, "Told to shutdown by the cancellationToken.");
        }

        private async Task HandleOneMessage(CancellationToken token, MsgWrapper wrapper)
        {
            if (wrapper == null) throw new ArgumentNullException(nameof(wrapper));

            bool saveWasAttempted = false;
            using (var session = _queue.BeginSession())
            {
                try
                {
                    await ReceiveSingleMessageAsync(wrapper, session);
                    saveWasAttempted = true;
                    await session.SaveChanges();
                    session.Dispose();
                    if (wrapper.Message == null)
                        await Task.Delay(100, token);
                }
                catch (SerializationException ex)
                {
                    _logger.Warning(_queue.Name, $"[attempt: {wrapper.AttemptCount}] Failed to deserialize message, throwing it away.", ex, wrapper.Message?.Principal, wrapper.Message?.Message);
                    await session.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Warning(_queue.Name, $"[attempt: {wrapper.AttemptCount}] Message handling failed.", ex, wrapper.Message?.Principal, wrapper.Message?.Message);

                    // do not retry at all, consume invalid messages
                    if (RetryAttempts.Length == 0)
                    {
                        PoisonMessageDetected?.Invoke(this,
                            new PoisonMessageEventArgs(wrapper.Message?.Principal, wrapper.Message?.Message, ex)
                            {
                                MessageQueue = _queue
                            });
                        if (!saveWasAttempted)
                        {
                            await session.SaveChanges();
                            session.Dispose();
                        }
                        await Task.Delay(1000, token);
                    }
                    else if (wrapper.AttemptCount < RetryAttempts.Length)
                    {
                        session.Dispose();
                        await Task.Delay(RetryAttempts[wrapper.AttemptCount], token);
                    }
                    else
                    {
                        _logger.Error(_queue.Name, "[attempt: {wrapper.AttemptCount}] Removing poison message.", ex, wrapper.Message?.Principal, wrapper.Message?.Message);

                        PoisonMessageDetected?.Invoke(this,
                            new PoisonMessageEventArgs(wrapper.Message?.Principal, wrapper.Message?.Message, ex)
                            {
                                MessageQueue = _queue
                            });
                        if (!saveWasAttempted)
                        {
                            await session.SaveChanges();
                        }

                        await Task.Delay(1000, token);
                    }
                }
            }
        }

        /// <summary>
        ///     Closing container scope.
        /// </summary>
        public event EventHandler<ScopeClosingEventArgs> ScopeClosing;

        /// <summary>
        ///     A new scope have been created for a message
        /// </summary>
        public event EventHandler<ScopeCreatedEventArgs> ScopeCreated;

        private async Task ProcessMessageAsync(DequeuedMessage msg)
        {
            var outboundMessages = new List<Message>();
            using (var scope = _scopeFactory.CreateScope())
            {
                _logger.Debug(_queue.Name, $"Created scope {scope.GetHashCode()}", msg.Principal, msg.Message);
                var e = new ScopeCreatedEventArgs(scope, msg.Principal, msg.Message);
                ScopeCreated?.Invoke(this, e);

                var invoker = MessageInvokerFactory == null
                    ? scope.ResolveDependency<IMessageInvoker>().First()
                    : MessageInvokerFactory(scope);
                var context = new InvocationContext(_queue.Name, msg.Principal, invoker, outboundMessages);

                await invoker.ProcessAsync(context, msg.Message);

                ScopeClosing?.Invoke(this, new ScopeClosingEventArgs(scope, msg.Message, e.ApplicationState) { Principal = e.Principal });
                _logger.Debug(_queue.Name, $"Closing scope {scope.GetHashCode()}.", msg.Principal, msg.Message);
            }

            if (msg.Principal == null)
                await _outboundRouter.SendAsync(outboundMessages);
            else
                await _outboundRouter.SendAsync(msg.Principal, outboundMessages);
        }

        private async Task ReceiveSingleMessageAsync(MsgWrapper wrapper, IMessageQueueSession session)
        {
            var msg = await session.DequeueWithCredentials(TimeSpan.FromSeconds(1));
            if (msg == null)
            {
                wrapper.Clear();
                return;
            }

            _logger.Debug(_queue.Name, "Received message " + msg.Message.Body.GetType().FullName, msg.Principal, msg.Message);

            wrapper.Assign(msg, _retryAttempts.Length);
            await ProcessMessageAsync(msg);
        }

        private class MsgWrapper
        {
            private int _maxAttempts;
            private DequeuedMessage _message;

            /// <summary>
            ///     0 when first attempt is made.
            /// </summary>
            public int AttemptCount { get; private set; }

            public Guid Id => Message?.Message.MessageId ?? Guid.Empty;

            public bool IsLastAttempt => AttemptCount >= _maxAttempts;

            public DequeuedMessage Message
            {
                get { return _message; }
            }

            public void Assign(DequeuedMessage message, int maxAttempts)
            {
                _maxAttempts = maxAttempts;
                _message = message;

                if (Id.Equals(message.Message.MessageId))
                    AttemptCount++;
                else
                    AttemptCount = 0;
            }

            public void Clear()
            {
                AttemptCount = 0;
                _message = null;
            }
        }
    }
}