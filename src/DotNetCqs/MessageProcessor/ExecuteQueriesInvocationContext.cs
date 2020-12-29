using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     Context
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Execute queries directly while all other messages are enqueued in the supplied bus.
    ///     </para>
    /// </remarks>
    public class ExecuteQueriesInvocationContext : IInvocationContext, IContainsQueueName
    {
        private readonly IMessageInvoker _messageInvoker;

        public ExecuteQueriesInvocationContext(ClaimsPrincipal principal, IMessageInvoker messageInvoker,
            string queueName)
        {
            Principal = principal;
            _messageInvoker = messageInvoker;
            QueueName = queueName;
        }

        public List<Message> Messages { get; } = new List<Message>();

        public List<Message> Replies { get; } = new List<Message>();

        public string QueueName { get; }

        public ClaimsPrincipal Principal { get; }

        public Task ReplyAsync(Message message)
        {
            Replies.Add(message);
#if NET452
            return Task.FromResult<object>(null);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task SendAsync(Message message)
        {
            Messages.Add(message);
        }

        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            var ctx = new ExecuteQueriesInvocationContext(Principal, _messageInvoker, QueueName);
            await _messageInvoker.ProcessAsync(ctx, query);
            return (TResult)ctx.Replies.First().Body;
        }
    }
}