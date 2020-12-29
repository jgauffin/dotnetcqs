using System;

namespace DotNetCqs.Logging
{
    /// <summary>
    /// Entry to log.
    /// </summary>
    public class LogEntry
    {
        public LogEntry(LogLevel level, string logMessage)
        {
            Level = level;
            LogMessage = logMessage;
        }

        /// <summary>
        /// Level to log
        /// </summary>
        public LogLevel Level { get; private set; }

        /// <summary>
        /// Queue that the message belongs to. (Optional parameter)
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Message type, typically the .NET full type name. (Optional parameter)
        /// </summary>
        public string MessageTypeName { get; set; }

        /// <summary>
        /// Message that are currently being processed. (Optional parameter)
        /// </summary>
        public object MessageBeingProcessed { get; set; }

        /// <summary>
        /// Log message
        /// </summary>
        public string LogMessage { get; private set; }

        /// <summary>
        /// Exception (if something failed).
        /// </summary>
        public Exception Exception { get; set; }
    }
}