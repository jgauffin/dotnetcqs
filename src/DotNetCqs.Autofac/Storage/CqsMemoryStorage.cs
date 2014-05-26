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

        public async Task<Command> PopCommandAsync()
        {
            Command cmd;
            return !_commandStorage.TryDequeue(out cmd) ? null : cmd;
        }

        public async Task<ApplicationEvent> PopEventAsync()
        {
            ApplicationEvent cmd;
            return !_eventStorage.TryDequeue(out cmd) ? null : cmd;
        }

        public async Task<IQuery> PopQueryAsync()
        {
            IQuery cmd;
            return !_queryStorage.TryDequeue(out cmd) ? null : cmd;
        }

        public async Task<IRequest> PopRequestAsync()
        {
            IRequest cmd;
            return !_requestStorage.TryDequeue(out cmd) ? null : cmd;
        }

        public async Task PushAsync(Command command)
        {
            _commandStorage.Enqueue(command);
        }

        public async Task PushAsync<T>(Request<T> request)
        {
            _requestStorage.Enqueue(request);
        }

        public async Task PushAsync(ApplicationEvent appEvent)
        {
            _eventStorage.Enqueue(appEvent);
        }

        public async Task PushAsync<T>(Query<T> query)
        {
            _queryStorage.Enqueue(query);
        }
    }
}