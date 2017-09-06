using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Autofac.Storage
{
    /// <summary>
    ///     Stores information in <c>ConcurrentQueue</c> classes.
    /// </summary>
    public class CqsMemoryStorage : ICqsStorage
    {
        private readonly ConcurrentQueue<Command> _commandStorage = new ConcurrentQueue<Command>();
        private readonly ConcurrentQueue<ApplicationEvent> _eventStorage = new ConcurrentQueue<ApplicationEvent>();
        private readonly ConcurrentQueue<IQuery> _queryStorage = new ConcurrentQueue<IQuery>();
        private readonly ConcurrentQueue<IRequest> _requestStorage = new ConcurrentQueue<IRequest>();

        public Encoding Encoding { get; set; }

        public Task<Command> PopCommandAsync()
        {
            return Task.FromResult(!_commandStorage.TryDequeue(out var cmd) ? null : cmd);
        }

        public Task<ApplicationEvent> PopEventAsync()
        {
            return Task.FromResult(!_eventStorage.TryDequeue(out var cmd) ? null : cmd);
        }

        public Task<IQuery> PopQueryAsync()
        {
            return Task.FromResult(!_queryStorage.TryDequeue(out var cmd) ? null : cmd);
        }

        public Task<IRequest> PopRequestAsync()
        {
            return Task.FromResult(!_requestStorage.TryDequeue(out var cmd) ? null : cmd);
        }

        public Task PushAsync(Command command)
        {
            _commandStorage.Enqueue(command);
            return Task.FromResult<object>(null);
        }

        public Task PushAsync<T>(Request<T> request)
        {
            _requestStorage.Enqueue(request);
            return Task.FromResult<object>(null);
        }

        public Task PushAsync(ApplicationEvent appEvent)
        {
            _eventStorage.Enqueue(appEvent);
            return Task.FromResult<object>(null);
        }

        public Task PushAsync<T>(Query<T> query)
        {
            _queryStorage.Enqueue(query);
            return Task.FromResult<object>(null);
        }
    }
}