using System.Threading.Tasks;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     Used to invoke message handlers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is used by the queue listener once a message arrives. The invocation method
    ///         is executed once per message.
    ///     </para>
    /// </remarks>
    public interface IMessageInvoker
    {
        /// <summary>
        ///     Execute handlers for the given message
        /// </summary>
        /// <param name="context">Contextual information and actions</param>
        /// <param name="message">Received message</param>
        /// <remarks>
        ///     <para>
        ///         Implementors should not suppress exceptions but let them flow or poison message handling and retry attempts.
        ///     </para>
        /// </remarks>
        Task ProcessAsync(IInvocationContext context, Message message);

        /// <summary>
        ///     Execute handlers for the given message
        /// </summary>
        /// <param name="context">Contextual information and actions</param>
        /// <param name="message">CQS object</param>
        /// <remarks>
        ///     <para>
        ///         Implementors should not suppress exceptions but let them flow or poison message handling and retry attempts.
        ///     </para>
        /// </remarks>
        Task ProcessAsync(IInvocationContext context, object message);
    }
}