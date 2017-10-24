using System;
using System.Security.Claims;

namespace DotNetCqs.Queues
{
    /// <summary>
    ///     A message that have been dequeued from a bus.
    /// </summary>
    public class DequeuedMessage
    {
        public DequeuedMessage(ClaimsPrincipal principal, Message message)
        {
            Principal = principal;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        ///     Dequeued message
        /// </summary>
        public Message Message { get; }

        /// <summary>
        ///     Principal that was attached to the message when enqueued. Can be <c>null</c>.
        /// </summary>
        public ClaimsPrincipal Principal { get; }
    }
}