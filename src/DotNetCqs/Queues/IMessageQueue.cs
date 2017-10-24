namespace DotNetCqs.Queues
{
    /// <summary>
    ///     Enqueue outbound messages
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        ///     Name of this queue.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Enqueue messages so that they can be processed later.
        /// </summary>
        /// <returns>session</returns>
        IMessageQueueSession BeginSession();
    }
}