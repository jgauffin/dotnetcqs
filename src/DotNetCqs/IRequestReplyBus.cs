using System;
using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// The request/reply bus is used to send the request and then wait for the reply.
    /// </summary>
    /// <seealso cref="Request{T}"/>
    public interface IRequestReplyBus
    {
        /// <summary>
        /// Invoke a request and wait for the reply
        /// </summary>
        /// <typeparam name="TReply">Type of reply that we should get for the request.</typeparam>
        /// <param name="request">Request that we want a reply for.</param>
        /// <returns>Task which will complete once we've got the result (or something failed, like a request wait timeout).</returns>
        /// <exception cref="ArgumentNullException">query</exception>
        Task<TReply> ExecuteAsync<TReply>(Request<TReply> request);
    }
}