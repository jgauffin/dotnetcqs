using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;

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
        private TimeSpan[] _retryAttempts;

        public LoggerHandler Logger;

        /// <summary>
        ///     Creates a new instance of <see cref="QueueListener" />.
        /// </summary>
        /// <param name="inboundQueue">Used to receive messages</param>
        /// <param name="scopeFactory"></param>
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
        public Task ReceiveSingleMessageAsync()
        {
            var wrapper = new MsgWrapper();
            return ReceiveSingleMessageAsync(wrapper);
        }

        public async Task RunAsync(CancellationToken token)
        {
            var wrapper = new MsgWrapper();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await ReceiveSingleMessageAsync(wrapper);
                }
                catch (Exception ex)
                {
                    Logger?.Invoke(LogLevel.Warning, _queue.Name, "Message handling failed, attempt: " + wrapper.AttemptCount + ", " + ex);

                    if (RetryAttempts.Length > wrapper.AttemptCount)
                        await Task.Delay(RetryAttempts[wrapper.AttemptCount], token);
                    else if (RetryAttempts.Length == 0)
                        await Task.Delay(1000, token);
                    else
                    {
                        Logger?.Invoke(LogLevel.Error, _queue.Name, "Removing poison message.");
                        using (var session = _queue.BeginSession())
                        {
                            var msg = await session.DequeueWithCredentials(TimeSpan.FromSeconds(1));
                            PoisonMessageDetected?.Invoke(this,
                                new PoisonMessageEventArgs(msg.Principal, msg.Message, ex));
                            await session.SaveChanges();
                        }
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
                Logger?.Invoke(LogLevel.Debug, _queue.Name, $"Created scope: {scope.GetHashCode()}");
                var e = new ScopeCreatedEventArgs(scope, msg.Principal, msg.Message);
                ScopeCreated?.Invoke(this, e);

                var invoker = scope.ResolveDependency<IMessageInvoker>().First();
                var context = new InvocationContext(_queue.Name, msg.Principal, invoker, outboundMessages);

                Logger?.Invoke(LogLevel.Debug, _queue.Name, "Invoking message handler(s).");
                await invoker.ProcessAsync(context, msg.Message);

                ScopeClosing?.Invoke(this, new ScopeClosingEventArgs(scope, msg.Message, e.ApplicationState));
                Logger?.Invoke(LogLevel.Debug, _queue.Name, $"Closing scope: {scope.GetHashCode()}");
            }

            if (msg.Principal == null)
                await _outboundRouter.SendAsync(outboundMessages);
            else
                await _outboundRouter.SendAsync(msg.Principal, outboundMessages);
        }

        private async Task ReceiveSingleMessageAsync(MsgWrapper wrapper)
        {
            using (var session = _queue.BeginSession())
            {
                var msg = await session.DequeueWithCredentials(TimeSpan.FromSeconds(1));
                if (msg == null)
                {
                    await session.SaveChanges();
                    wrapper.Clear();
                    await Task.Delay(100);
                    return;
                }

                if (msg.Principal != null)
                    Logger?.Invoke(LogLevel.Info, _queue.Name, $"Received[{msg.Principal.Identity.Name}]: {msg.Message.Body}");
                else
                    Logger?.Invoke(LogLevel.Info, _queue.Name, $"Received[Anonymous]: {msg.Message.Body}");
                wrapper.Assign(msg.Message.MessageId);
                await ProcessMessageAsync(msg);
                await session.SaveChanges();
            }
        }

        private class MsgWrapper
        {
            /// <summary>
            ///     0 when first attempt is made.
            /// </summary>
            public int AttemptCount { get; private set; }

            public Guid Id { get; private set; }

            public void Assign(Guid id)
            {
                if (Id == id)
                {
                    AttemptCount++;
                }
                else
                {
                    Id = id;
                    AttemptCount = 0;
                }
            }

            public void Clear()
            {
                AttemptCount = 0;
                Id = Guid.Empty;
            }
        }
    }
}