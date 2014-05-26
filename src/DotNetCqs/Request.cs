using System;

namespace DotNetCqs
{
    /// <summary>
    /// Request base class (for Request/Reply). 
    /// </summary>
    /// <typeparam name="TReply">Type of reply that the request will get.</typeparam>
    /// <remarks>
    /// <para>
    /// Sometimes you can't avoid to wait on an reply before continuing. Typically when the server side generates some kind of information (other than an ID). It's in these cases
    /// that you should use Request/Reply. Try to use <see cref="Command"/> otherwise as it gives much better performance.
    /// </para>
    /// <para>Uses <see cref="GuidFactory"/> to assign the <see cref="RequestId"/>.</para>
    /// </remarks>
    public class Request<TReply> : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Request{TResult}"/> class.
        /// </summary>
        public Request()
        {
            RequestId = GuidFactory.Create();
        }

        /// <summary>
        /// Gets unique identifier for this request (so that we can identify replies).
        /// </summary>
        public Guid RequestId { get; private set; }
    }
}
