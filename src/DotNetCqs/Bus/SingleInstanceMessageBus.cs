using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Queues;

namespace DotNetCqs.Bus
{
    /// <summary>
    ///     Message bus designed to be instantiated during the entire application lifetime (each method invocation is wrapped
    ///     in a message session).
    /// </summary>
    public class SingleInstanceMessageBus : IMessageBus
    {
        private readonly IMessageQueue _messageQueue;

        /// <summary>
        ///     Creates a new instance of <see cref="SingleInstanceMessageBus" />
        /// </summary>
        /// <param name="messageQueue">All messages are enqueued directly</param>
        /// <exception cref="ArgumentNullException">messageQueue</exception>
        public SingleInstanceMessageBus(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        }


        /// <inheritdoc />
        public Task SendAsync(ClaimsPrincipal principal, object message)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (message == null) throw new ArgumentNullException(nameof(message));
            return SendAsync(principal, new Message(message));
        }

        /// <inheritdoc />
        public async Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (message == null) throw new ArgumentNullException(nameof(message));
            using (var session = _messageQueue.BeginSession())
            {
                await session.EnqueueAsync(principal, message);
                await session.SaveChanges();
            }
        }

        /// <inheritdoc />
        public async Task SendAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            using (var session = _messageQueue.BeginSession())
            {
                await session.EnqueueAsync(message);
                await session.SaveChanges();
            }
        }

        /// <inheritdoc />
        public Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return SendAsync(new Message(message));
        }
    }
}