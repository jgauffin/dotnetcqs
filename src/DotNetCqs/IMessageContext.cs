using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    ///     Details regarding the message
    /// </summary>
    public interface IMessageContext
    {
        /// <summary>
        ///     Id of the inbound message (if supported by the underlying messaging system, otherwise <c>Guid.Empty</c>)
        /// </summary>
        Guid MessageId { get; }

        /// <summary>
        ///     Principal that sent the message (can be a system user)
        /// </summary>
        ClaimsPrincipal Principal { get; }

        /// <summary>
        ///     Properties that was transported with the <see cref="Message" />.
        /// </summary>
        IDictionary<string, string> Properties { get; set; }

        /// <summary>
        ///     Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="query">QueryAsync to execute.</param>
        /// <returns>Task which will complete once we've got the result (or something failed, like a query wait timeout).</returns>
        /// <exception cref="ArgumentNullException">query</exception>
        Task<TResult> QueryAsync<TResult>(Query<TResult> query);

        /// <summary>
        ///     Reply (uses the inbound MessageId as correlationId)
        /// </summary>
        /// <param name="message">message</param>
        Task ReplyAsync(object message);

        /// <summary>
        ///     Send a new message (unrelated to the current one)
        /// </summary>
        /// <param name="message">message</param>
        Task SendAsync(object message);

        /// <summary>
        ///     Send a new message (unrelated to the current one)
        /// </summary>
        /// <param name="message">message</param>
        Task SendAsync(Message message);
    }
}