using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Queues;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     Context for the queue listener.
    /// </summary>
    public class InvocationContext : IInvocationContext
    {
        private readonly IMessageInvoker _messageInvoker;
        private readonly IMessageQueueSession _session;

        public InvocationContext(ClaimsPrincipal principal, IMessageQueueSession session,
            IMessageInvoker messageInvoker)
        {
            Principal = principal;
            _session = session;
            _messageInvoker = messageInvoker;
        }

        public Guid MessageId { get; set; }

        public ClaimsPrincipal Principal { get; }

        public async Task ReplyAsync(Message message)
        {
            if (message.CorrelationId == Guid.Empty)
                message.CorrelationId = MessageId;
            if (message.MessageId == Guid.Empty)
                message.MessageId = GuidFactory.Create();

            await _session.EnqueueAsync(Principal, message);
        }

        public async Task SendAsync(Message message)
        {
            if (message.MessageId == Guid.Empty)
                message.MessageId = GuidFactory.Create();

            await _session.EnqueueAsync(Principal, message);
        }

        public Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            //TODO: Send message on queue and wait for response in the reply queue (filtering on correlationid)

            var ctx = new ExecuteQueriesInvocationContext(Principal, _messageInvoker);
            return ctx.QueryAsync(query);
        }
    }
}