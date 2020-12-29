using System;

namespace DotNetCqs.Logging
{
    /// <summary>
    /// Logging abstraction.
    /// </summary>
    public interface ILogger
    {

        public void Write(LogLevel level, string message);
        public void Write(LogLevel level, string message, Exception exception);
        public void Write(LogEntry entry);
    }
}