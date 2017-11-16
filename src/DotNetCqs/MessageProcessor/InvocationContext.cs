using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<Message> _outboundMessages;

        public InvocationContext(string queueName, ClaimsPrincipal principal, IMessageInvoker messageInvoker, List<Message> outboundMessages)
        {
            Principal = principal;
            QueueName = queueName;
            _messageInvoker = messageInvoker;
            _outboundMessages = outboundMessages ?? throw new ArgumentNullException(nameof(outboundMessages));
        }

        /// <summary>
        /// current message id
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Queue that we read from
        /// </summary>
        public string QueueName { get; }


        /// <inheritdoc />
        public ClaimsPrincipal Principal { get; }

        /// <summary>
        /// Messages are enqueued and delivered after the current message have been processed.
        /// </summary>
        /// <param name="message">message to send</param>
        /// <returns>task</returns>
        public Task ReplyAsync(Message message)
        {
            if (message.CorrelationId == Guid.Empty)
                message.CorrelationId = MessageId;
            if (message.MessageId == Guid.Empty)
                message.MessageId = GuidFactory.Create();

            _outboundMessages.Add(message);
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Messages are enqueued and delivered after the current message have been processed.
        /// </summary>
        /// <param name="message">message to send</param>
        /// <returns>task</returns>
        public Task SendAsync(Message message)
        {
            if (message.MessageId == Guid.Empty)
                message.MessageId = GuidFactory.Create();

            _outboundMessages.Add(message);
            return Task.FromResult<object>(null);
        }

        public Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            //TODO: Send message on queue and wait for response in the reply queue (filtering on correlation-id)

            var ctx = new ExecuteQueriesInvocationContext(Principal, _messageInvoker);
            return ctx.QueryAsync(query);
        }

        public void DeliverMessages(IMessageQueue queue)
        {
            if (_outboundMessages.Count == 0)
                return;

            using (var scope = queue.BeginSession())
            {
                scope.EnqueueAsync(Principal, _outboundMessages);
                _outboundMessages.Clear();
            }
        }
    }
}