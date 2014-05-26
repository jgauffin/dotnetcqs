using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace DotNetCqs.Autofac
{
    /// <summary>
    ///     Event bus implementation using autofac as a container.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This bus will store all events on disk and execute them on another thread. This means that the events will be
    ///         executed even if the application
    ///         crashes (unless the application crashes during the actual execution of the event).
    ///     </para>
    /// </remarks>
    public class ContainerCommandBus : ICommandBus, IDisposable, IStartable
    {
        private readonly IContainer _container;
        private readonly Thread _executionThread;
        private readonly ICqsStorage _cqsStorage;
        private readonly ManualResetEventSlim _jobEvent = new ManualResetEventSlim(false);
        private bool _shutdown;

        public ContainerCommandBus(ICqsStorage cqsStorage, IContainer container)
        {
            if (cqsStorage == null) throw new ArgumentNullException("cqsStorage");
            if (container == null) throw new ArgumentNullException("container");

            _cqsStorage = cqsStorage;
            _container = container;
            _executionThread = new Thread(OnExecuteCommand);
        }

        /// <summary>
        ///     Request that a command should be executed.
        /// </summary>
        /// <typeparam name="T">Type of command to execute.</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>
        ///     Task which completes once the command has been delivered (and NOT when it has been executed).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">command</exception>
        /// <remarks>
        ///     <para>
        ///         The actual execution of an command can be done anywhere at any time. Do not expect the command to be executed
        ///         just because this method returns. That just means
        ///         that the command have been successfully delivered (to a queue or another process etc) for execution.
        ///     </para>
        /// </remarks>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            await _cqsStorage.PushAsync(command);
            _jobEvent.Set();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        ///     A specific handler failed to consume the application event.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         We will not try to invoke the event again as one or more handlers may have consumed the event successfully.
        ///     </para>
        /// </remarks>
        public event EventHandler<CommandHandlerFailedEventArgs> HandlerFailed = delegate { };

        /// <summary>
        ///     Bus failed to invoke an event.
        /// </summary>
        public event EventHandler<BusFailedEventArgs> BusFailed = delegate { };

        /// <summary>
        /// Start bus (required to start processing queued commands)
        /// </summary>
        public void Start()
        {
            _jobEvent.Reset();
            _shutdown = false;
            _executionThread.Start();
        }

        /// <summary>
        /// Stop processing bus (will wait for the current command to be completed before shutting down)
        /// </summary>
        public void Stop()
        {
            _shutdown = true;
            _jobEvent.Set();
            _executionThread.Join(5000);
        }

        /// <summary>
        /// </summary>
        /// <returns><c>true</c> if we found a command to execute.</returns>
        internal async Task<bool> ExecuteJobAsync()
        {
            var command = await _cqsStorage.PopCommandAsync();
            if (command == null)
                return false;

            using (var scope = _container.BeginLifetimeScope())
            {
                var type = typeof (ICommandHandler<>).MakeGenericType(command.GetType());
                var collectionType = typeof (IEnumerable<>).MakeGenericType(type);
                var handlers = ((IEnumerable<object>) scope.Resolve(collectionType)).ToArray();

                if (handlers.Length == 0)
                    throw new CqsHandlerMissingException(command.GetType());
                if (handlers.Length != 1)
                    throw new OnlyOneHandlerAllowedException(command.GetType());

                try
                {
                    var method = type.GetMethod("ExecuteAsync");
                    var task  = (Task) method.Invoke(handlers[0], new object[] {command});
                    await task;
                }
                catch (Exception exception)
                {
                    if (exception is TargetInvocationException)
                        exception = exception.InnerException;
                    HandlerFailed(this, new CommandHandlerFailedEventArgs(command, handlers[0], exception));
                }
            }

            return true;
        }

        private void OnExecuteCommand()
        {
            while (true)
            {
                _jobEvent.Wait(1000);
                if (_shutdown)
                    return;

                try
                {
                    var task = ExecuteJobAsync();
                    task.Wait();
                    if (!task.Result)
                        _jobEvent.Reset();
                }
                catch (Exception exception)
                {
                    BusFailed(this, new BusFailedEventArgs(exception));
                }
            }
        }
    }
}