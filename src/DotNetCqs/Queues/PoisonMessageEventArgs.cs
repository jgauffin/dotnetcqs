using System;
using System.Security.Claims;

namespace DotNetCqs.Queues
{
    /// <summary>
    ///     Tried to process a message the maximum number of attempts.
    /// </summary>
    public class PoisonMessageEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of <see cref="PoisonMessageEventArgs" />.
        /// </summary>
        /// <param name="principal">Principal that was attached to the message (if any)</param>
        /// <param name="message">Message that could not be processed.</param>
        /// <param name="exception">Reason to why it couldn't be processed.</param>
        public PoisonMessageEventArgs(ClaimsPrincipal principal, Message message, Exception exception)
        {
            Principal = principal;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        ///     Reason to why it couldn't be processed.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     Message that could not be processed.
        /// </summary>
        public Message Message { get; }

        /// <summary>
        ///     Principal that was attached to the message (if any)
        /// </summary>
        public ClaimsPrincipal Principal { get; }

        /// <summary>
        /// Queue that the message was in.
        /// </summary>
        public IMessageQueue MessageQueue { get; set; }
    }
}