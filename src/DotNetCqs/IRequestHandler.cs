using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// Used to execute a request and generate the reply.
    /// </summary>
    /// <typeparam name="TRequest">Type of request</typeparam>
    /// <typeparam name="TReply">Type of reply</typeparam>
    public interface IRequestHandler<in TRequest, TReply> where TRequest : Request<TReply>
    {
        /// <summary>
        /// Execute the request and generate a reply.
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>Task which will contain the reply once completed.</returns>
        Task<TReply> ExecuteAsync(TRequest request);
    }
}