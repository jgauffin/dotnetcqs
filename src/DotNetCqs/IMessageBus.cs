using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    ///     Enqueue messages in the underlying queue system.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A message bus typically do not execute messages, but schedule them for execution in another part
    ///         of the system.
    ///     </para>
    /// </remarks>
    public interface IMessageBus
    {
        /// <summary>
        ///     Send a message
        /// </summary>
        /// <param name="principal">
        ///     Authenticated user (or a system account to indicate background services)
        /// </param>
        /// <param name="message">message, will be wrapped in <see cref="Message" />.</param>
        /// <exception cref="ArgumentNullException">principal;message</exception>
        Task SendAsync(ClaimsPrincipal principal, object message);

        /// <summary>
        ///     Send a message
        /// </summary>
        /// <param name="principal">
        ///     Authenticated user (or a system account to indicate background services)
        /// </param>
        /// <param name="message">message</param>
        /// <exception cref="ArgumentNullException">principal;message</exception>
        Task SendAsync(ClaimsPrincipal principal, Message message);

        /// <summary>
        ///     Send a message
        /// </summary>
        /// <param name="message">message</param>
        /// <remarks>
        ///     <para>
        ///         By using this method you say that the user (authorization) does not matter and that the message is without
        ///         context.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">message</exception>
        Task SendAsync(Message message);

        /// <summary>
        ///     Send a message
        /// </summary>
        /// <param name="message">message, will be wrapped in <see cref="Message" />.</param>
        /// <remarks>
        ///     <para>
        ///         By using this method you say that the user (authorization) does not matter and that the message is without
        ///         context.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">principal;message</exception>
        Task SendAsync(object message);
    }
}