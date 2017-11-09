using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs.Queues
{
    /// <summary>
    ///     Session for <see cref="IMessageQueue" />.
    /// </summary>
    public interface IMessageQueueSession : IDisposable
    {
        /// <summary>
        ///     Dequeue a message
        /// </summary>
        /// <param name="suggestedWaitPeriod">
        ///     Amount of time to wait for a new message to arrive when no message are available (if
        ///     supported)
        /// </param>
        /// <returns><c>message</c> if a message was available; <c>null</c> if the queue was  empty.</returns>
        Task<Message> Dequeue(TimeSpan suggestedWaitPeriod);

        /// <summary>
        ///     Dequeue a message
        /// </summary>
        /// <param name="suggestedWaitPeriod">
        ///     Amount of time to wait for a new message to arrive when no message are available (if
        ///     supported)
        /// </param>
        /// <returns><c>message</c> if a message was available; <c>null</c> if the queue was  empty.</returns>
        /// <remarks>
        ///     <para>
        ///         Credentials will be <c>null</c> if the enqueued message did not include them.
        ///     </para>
        /// </remarks>
        Task<DequeuedMessage> DequeueWithCredentials(TimeSpan suggestedWaitPeriod);

        /// <summary>
        ///     Enqueue message so that they can be processed later.
        /// </summary>
        /// <param name="principal">Principal that should be attached with the message (typically the logged in principal).</param>
        /// <param name="messages">Messages that was enqueued by the current message handler</param>
        Task EnqueueAsync(ClaimsPrincipal principal, IReadOnlyCollection<Message> messages);

        /// <summary>
        ///     Enqueue message so that they can be processed later.
        /// </summary>
        /// <param name="messages">Messages that was enqueued by the current message handler</param>
        Task EnqueueAsync(IReadOnlyCollection<Message> messages);

        /// <summary>
        ///     Enqueue message so that they can be processed later.
        /// </summary>
        /// <param name="principal">Principal that should be attached with the message (typically the logged in principal).</param>
        /// <param name="message">Messages that was enqueued by the current message handler</param>
        Task EnqueueAsync(ClaimsPrincipal principal, Message message);

        /// <summary>
        ///     Enqueue message so that they can be processed later.
        /// </summary>
        /// <param name="message">Messages that was enqueued by the current message handler</param>
        Task EnqueueAsync(Message message);

        /// <summary>
        ///     Commits message to queue
        /// </summary>
        Task SaveChanges();
    }
}