using System;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     A handler failed to process a message.
    /// </summary>
    public class HandlerFailedException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <see cref="HandlersFailedException" />.
        /// </summary>
        /// <param name="message">Message that could not be processed.</param>
        /// <param name="handlerType">Handler that could not process the message successfully.</param>
        /// <param name="innerException">Reason to why the handler failed.</param>
        public HandlerFailedException(Message message, Type handlerType, Exception innerException)
            : base($"'{handlerType}' failed to process '{message.Body}'.", innerException)
        {
            ProcessedMessage = message;
            HandlerType = handlerType;
        }

        /// <summary>
        ///     Handler that could not process the message successfully.
        /// </summary>
        public Type HandlerType { get; }

        /// <summary>
        ///     Message that could not be processed.
        /// </summary>
        public Message ProcessedMessage { get; }
    }
}