using System;

namespace DotNetCqs.Logging
{
    /// <summary>
    /// Writes entries to the console.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void Write(LogLevel level, string message)
        {
            Write(new LogEntry(level, message));
        }

        public void Write(LogLevel level, string message, Exception exception)
        {
            Write(new LogEntry(level, message) {Exception = exception});
        }

        public void Write(LogEntry entry)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = GetLevelColor(entry);

            var msg = "";
            if (!string.IsNullOrEmpty(entry.QueueName))
                msg += $"[{entry.QueueName}] ";

            if (entry.MessageBeingProcessed != null)
                msg += $"({entry.MessageBeingProcessed})";
            else if (!string.IsNullOrEmpty(entry.MessageTypeName))
                msg += $"({entry.MessageTypeName}) ";

            msg += entry.LogMessage;
            if (entry.Exception != null)
                msg += "\r\n" + entry.Exception;

            Console.WriteLine(msg);
            Console.ForegroundColor = color;
        }

        private static ConsoleColor GetLevelColor(LogEntry entry)
        {
            switch (entry.Level)
            {
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    return ConsoleColor.Gray;
                    break;
                case LogLevel.Warning:
                    return ConsoleColor.Magenta;
                    break;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                    break;
            }
        }
    }
}