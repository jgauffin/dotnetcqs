using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    ///     Used to process a message
    /// </summary>
    /// <typeparam name="TMessage">Message type</typeparam>
    public interface IMessageHandler<in TMessage>
    {
        /// <summary>
        ///     Handle an inbound message
        /// </summary>
        /// <param name="context">context (like who sent the message or how to send/reply)</param>
        /// <param name="message">the message</param>
        Task HandleAsync(IMessageContext context, TMessage message);
    }
}