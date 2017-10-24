using System;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     Information about a handler that failed to process a message
    /// </summary>
    public class FailedHandler
    {
        /// <summary>
        ///     Message that could not be processed.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Handler that failed
        /// </summary>
        public Type HandlerType { get; set; }
    }
}