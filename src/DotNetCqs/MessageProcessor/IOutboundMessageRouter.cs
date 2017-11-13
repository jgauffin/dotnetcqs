using System.Threading.Tasks;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     Deliver messages that have been enqueued by an invoked <see cref="IMessageHandler{TMessage}" />.
    /// </summary>
    public interface IOutboundMessageRouter
    {
        /// <summary>
        ///     Deliver all or some of the messages manually.
        /// </summary>
        /// <param name="msgsToDeliver">Messages enqueued by a handler</param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         You must remove all delivered messages from <see cref="IOutboundMessages.OutboundMessages" /> and
        ///         <see cref="IOutboundMessages.Replies" /> as
        ///         all remaining messages will be delivered by the default handling.
        ///     </para>
        /// </remarks>
        Task SendAsync(IOutboundMessages msgsToDeliver);
    }
}