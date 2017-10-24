using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     Context for <see cref="IMessageInvoker" />
    /// </summary>
    public interface IInvocationContext
    {
        /// <summary>
        ///     Principal that arrived with the message (can be null)
        /// </summary>
        ClaimsPrincipal Principal { get; }

        Task<TResult> QueryAsync<TResult>(Query<TResult> query);

        /// <summary>
        ///     Reply to the current message (typically to queries)
        /// </summary>
        /// <param name="message"></param>
        /// <returns>task</returns>
        /// <remarks>
        ///     <para><c>CorrelationId</c> and <c>MessageId</c> will be specified if <c>null</c>.</para>
        /// </remarks>
        Task ReplyAsync(Message message);

        /// <summary>
        ///     Enqueue another message for delivery
        /// </summary>
        /// <param name="message">Outbound message</param>
        /// <returns>task</returns>
        /// <remarks>
        ///     <para>
        ///         Message will not be executed but just enqueued.
        ///     </para>
        /// </remarks>
        Task SendAsync(Message message);
    }
}