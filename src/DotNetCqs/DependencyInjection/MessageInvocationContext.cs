using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.MessageProcessor;

namespace DotNetCqs.DependencyInjection
{
    /// <summary>
    ///     Message context used when a message is being processed by it's message handlers.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <seealso cref="IMessageInvoker" />
    public class MessageInvocationContext : IMessageContext
    {
        private readonly IMessageInvoker _messageInvoker;
        private readonly List<Message> _outboundMessages = new List<Message>();
        private readonly List<Message> _replies = new List<Message>();

        /// <summary>
        ///     Creates a new instance of <see cref="MessageInvocationContext" />
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="message"></param>
        /// <param name="messageInvoker"></param>
        public MessageInvocationContext(ClaimsPrincipal principal, Message message, IMessageInvoker messageInvoker)
        {
            _messageInvoker = messageInvoker;
            Principal = principal;
            foreach (var key in message.Properties.Keys)
                Properties[key] = message.Properties[key];

            MessageId = message.MessageId;
        }

        public IReadOnlyList<Message> OutboundMessages => _outboundMessages;
        public IReadOnlyList<Message> Replies => _replies;

        public Guid MessageId { get; }
        public IDictionary<string, string> Properties { get; set; }
        public ClaimsPrincipal Principal { get; set; }

        public Task ReplyAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var msg = new Message(message) {CorrelationId = MessageId};
            _replies.Add(msg);
#if NET452
            return Task.FromResult<object>(null);
#else
            return Task.CompletedTask;
#endif
        }

        public Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var msg = new Message(message);
            return SendAsync(msg);
        }

        public Task SendAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _outboundMessages.Add(message);
#if NET452
            return Task.FromResult<object>(null);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            var ctx = new ExecuteQueriesInvocationContext(Principal, _messageInvoker);
            return await ctx.QueryAsync(query);
        }
    }
}