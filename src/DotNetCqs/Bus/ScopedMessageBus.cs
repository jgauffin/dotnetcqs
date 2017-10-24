using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Queues;

namespace DotNetCqs.Bus
{
    /// <summary>
    ///     Message bus designed to be invoked in a scoped execution path.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All messages are stored internally and are not put on the queue until
    ///         <see cref="M:DotNetCqs.Bus.ScopedMessageBus.CommitAsync" /> is invoked.
    ///     </para>
    /// </remarks>
    public class ScopedMessageBus : IMessageBus
    {
        private readonly IMessageQueue _messageQueue;
        private readonly List<Tuple<ClaimsPrincipal, Message>> _messages = new List<Tuple<ClaimsPrincipal, Message>>();

        /// <summary>
        ///     Creates a new instance of <see cref="ScopedMessageBus" />
        /// </summary>
        /// <param name="messageQueue">Queue to put all messages in</param>
        /// <exception cref="ArgumentNullException">messageQueue</exception>
        public ScopedMessageBus(IMessageQueue messageQueue)
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
        public Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (message == null) throw new ArgumentNullException(nameof(message));
            _messages.Add(new Tuple<ClaimsPrincipal, Message>(principal, message));
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task SendAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _messages.Add(new Tuple<ClaimsPrincipal, Message>(null, message));
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task SendAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return SendAsync(new Message(message));
        }

        /// <summary>
        ///     Commit all enqueued messages.
        /// </summary>
        /// <returns>task</returns>
        public async Task CommitAsync()
        {
            using (var scope = _messageQueue.BeginSession())
            {
                foreach (var perClaim in _messages.GroupBy(x => x.Item1))
                {
                    if (perClaim.Key == null)
                        await scope.EnqueueAsync(perClaim.Select(x => x.Item2).ToList());
                    else
                        await scope.EnqueueAsync(perClaim.Key, perClaim.Select(x => x.Item2).ToList());
                }

                await scope.SaveChanges();
            }
        }
    }
}