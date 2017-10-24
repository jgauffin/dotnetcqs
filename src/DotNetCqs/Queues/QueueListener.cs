using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;

namespace DotNetCqs.Queues
{
    /// <summary>
    ///     Listens on inbound queue to be able to process messages.
    /// </summary>
    public class QueueListener
    {
        private readonly IMessageQueue _queue;
        private readonly IHandlerScopeFactory _scopeFactory;
        private TimeSpan[] _retryAttempts;


        public Action<string> Logger;

        /// <summary>
        ///     Creates a new instance of <see cref="QueueListener" />.
        /// </summary>
        /// <param name="queue">Used to receive messages</param>
        /// <param name="scopeFactory"></param>
        public QueueListener(IMessageQueue queue, IHandlerScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task ProcessSingleMessageAsync()
        {
            using (var session = _queue.BeginSession())
            {
                var msg = await session.DequeueWithCredentials(TimeSpan.FromSeconds(1));
                if (msg == null)
                {
                    await session.SaveChanges();
                    return;
                }
                Logger?.Invoke(_queue.Name + " Msg: " + msg.Message.Body);

                using (var scope = _scopeFactory.CreateScope())
                {
                    Logger?.Invoke(_queue.Name + " Created scope: " + scope.GetHashCode());
                    var e = new ScopeCreatedEventArgs(scope, msg.Principal, msg.Message);
                    ScopeCreated?.Invoke(this, e);

                    var invoker = scope.ResolveDependency<IMessageInvoker>().First();
                    var context = new InvocationContext(msg.Principal, session, invoker);

                    Logger?.Invoke(_queue.Name + "  Invoking handler(s).");
                    await invoker.ProcessAsync(context, msg.Message);

                    ScopeClosing?.Invoke(this, new ScopeClosingEventArgs(scope, msg.Message, e.ApplicationState));
                    Logger?.Invoke(_queue.Name + "  Closing scope: " + scope.GetHashCode());
                }

                await session.SaveChanges();
            }
        }

        public async Task RunAsync(CancellationToken token)
        {
            var attemptNumber = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await ProcessSingleMessageAsync();
                    attemptNumber = 0;
                }
                catch (Exception ex)
                {
                    if (RetryAttempts.Length > attemptNumber)
                        await Task.Delay(RetryAttempts[attemptNumber], token);
                    else if (RetryAttempts.Length == 0)
                        await Task.Delay(1000, token);
                    else
                        using (var session = _queue.BeginSession())
                        {
                            var msg = await session.DequeueWithCredentials(TimeSpan.FromSeconds(1));
                            PoisonMessageDetected?.Invoke(this,
                                new PoisonMessageEventArgs(msg.Principal, msg.Message, ex));
                            await session.SaveChanges();
                        }
                    ++attemptNumber;
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
    }
}