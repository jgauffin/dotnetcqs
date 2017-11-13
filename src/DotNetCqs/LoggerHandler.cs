using DotNetCqs.Queues;

namespace DotNetCqs
{
    public delegate void LoggerHandler(LogLevel level, string queueNameOrMessageName, string message);
}