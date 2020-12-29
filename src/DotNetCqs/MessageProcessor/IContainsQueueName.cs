namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    /// Attaches queue name name to context objects.
    /// </summary>
    public interface IContainsQueueName
    {
        /// <summary>
        /// Queue being processed.
        /// </summary>
        string QueueName { get; }
    }
}