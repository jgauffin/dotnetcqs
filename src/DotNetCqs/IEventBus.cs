using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// Used to deliver application events.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publish a new application event.
        /// </summary>
        /// <typeparam name="TApplicationEvent">Type of event to publish.</typeparam>
        /// <param name="e">Event to publish, must be serializable.</param>
        /// <returns>Task triggered once the event has been delivered.</returns>
        Task PublishAsync<TApplicationEvent>(TApplicationEvent e) where TApplicationEvent : ApplicationEvent;
    }
}
