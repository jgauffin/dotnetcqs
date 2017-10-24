using System;
using System.Collections.Generic;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     One or more handlers failed to process a message.
    /// </summary>
    public class HandlersFailedException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <see cref="HandlersFailedException" />.
        /// </summary>
        /// <param name="message">Message that triggered the failure</param>
        /// <param name="failedHandlers">Handlers that failed</param>
        public HandlersFailedException(Message message, IReadOnlyList<FailedHandler> failedHandlers)
            : base($"Message handlers failed for '{message.Body}'.", failedHandlers[0].Exception)
        {
            ProcessedMessage = message;
            FailedHandlers = failedHandlers;
        }

        /// <summary>
        ///     Handlers that failed.
        /// </summary>
        public IReadOnlyList<FailedHandler> FailedHandlers { get; }

        /// <summary>
        ///     Message that triggered the failure.
        /// </summary>
        public Message ProcessedMessage { get; }
    }
}