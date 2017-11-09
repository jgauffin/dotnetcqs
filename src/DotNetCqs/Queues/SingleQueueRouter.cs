using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs.Queues
{
    /// <summary>
    ///     Sends all messages through the same message queue
    /// </summary>
    public class SingleQueueRouter : IMessageRouter
    {
        private IMessageQueue _messageQueue;

        public SingleQueueRouter(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        }

        public async Task SendAsync(Message message)
        {
            using (var session = _messageQueue.BeginSession())
            {
                await session.EnqueueAsync(message);
                await session.SaveChanges();
            }
        }

        public async Task SendAsync(IReadOnlyCollection<Message> messages)
        {
            using (var session = _messageQueue.BeginSession())
            {
                await session.EnqueueAsync(messages);
                await session.SaveChanges();
            }
        }

        public async Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            using (var session = _messageQueue.BeginSession())
            {
                await session.EnqueueAsync(principal, message);
                await session.SaveChanges();
            }
        }

        public async Task SendAsync(ClaimsPrincipal principal, IReadOnlyCollection<Message> messages)
        {
            using (var session = _messageQueue.BeginSession())
            {
                await session.EnqueueAsync(principal, messages);
                await session.SaveChanges();
            }
        }
    }
}