using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// Subscribes on application events.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to subscribe on</typeparam>
    /// <remarks>
    /// <para>Handlers are typically discovered by using an inversion of control container.</para>
    /// </remarks>
    public interface IApplicationEventSubscriber<in TEvent> where TEvent : ApplicationEvent
    {
        /// <summary>
        /// Process an event asynchronously.
        /// </summary>
        /// <param name="e">event to process</param>
        /// <returns>Task to wait on.</returns>
        Task HandleAsync(TEvent e);
    }
}