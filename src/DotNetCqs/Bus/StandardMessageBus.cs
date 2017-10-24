using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Queues;

namespace DotNetCqs.Bus
{
    /// <summary>
    ///     Message bus designed to be invoked in a scoped
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class StandardMessageBus : IMessageBus
    {
        private readonly IMessageQueue _messageQueue;
        private readonly List<Tuple<ClaimsPrincipal, Message>> _messages = new List<Tuple<ClaimsPrincipal, Message>>();

        public StandardMessageBus(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        }


        /// <summary>
        ///     Enqueue message for delivery (distribute them by using <see cref="CommitAsync" />).
        /// </summary>
        /// <param name="principal">Authenticated user</param>
        /// <param name="message">Message to enqueue, will be wrapped in a <see cref="Message" />.</param>
        /// <returns>task</returns>
        public Task SendAsync(ClaimsPrincipal principal, object message)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (message == null) throw new ArgumentNullException(nameof(message));
            _messages.Add(new Tuple<ClaimsPrincipal, Message>(principal, new Message(message)));
            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Enqueue message for delivery (distribute them by using <see cref="CommitAsync" />).
        /// </summary>
        /// <param name="principal">Authenticated user</param>
        /// <param name="message">Message to enqueue, will be wrapped in a <see cref="Message" />.</param>
        /// <returns>task</returns>
        public Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (message == null) throw new ArgumentNullException(nameof(message));
            _messages.Add(new Tuple<ClaimsPrincipal, Message>(principal, message));
            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Enqueue message for delivery (distribute them by using <see cref="CommitAsync" />).
        /// </summary>
        /// <param name="message">Message to enqueue, will be wrapped in a <see cref="Message" />.</param>
        /// <returns>task</returns>
        public Task SendAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _messages.Add(new Tuple<ClaimsPrincipal, Message>(null, message));
            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Enqueue message for delivery (distribute them by using <see cref="CommitAsync" />).
        /// </summary>
        /// <param name="message">Message to enqueue, will be wrapped in a <see cref="Message" />.</param>
        /// <returns>task</returns>
        public Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _messages.Add(new Tuple<ClaimsPrincipal, Message>(null, new Message(message)));
            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Enqueue all messages.
        /// </summary>
        /// <returns>task</returns>
        public async Task CommitAsync()
        {
            var grouped = _messages.GroupBy(x => x.Item1);
            using (var session = _messageQueue.BeginSession())
            {
                foreach (var group in grouped)
                {
                    if (group.Key == null)
                        await session.EnqueueAsync(group.Select(x => x.Item2).ToList());
                    else
                        await session.EnqueueAsync(group.Key, group.Select(x => x.Item2).ToList());
                }

                await session.SaveChanges();
            }
        }
    }
}