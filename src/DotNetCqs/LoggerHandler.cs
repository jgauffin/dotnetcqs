namespace DotNetCqs.Queues
{
    public delegate void LoggerHandler(LogLevel level, string queueNameOrMessageName, string message);
}