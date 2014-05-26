using System;
using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// The query bus is used to send query requests and wait for the answer.
    /// </summary>
    /// <remarks>
    /// <para>What queries are is defined by the <see cref="IQuery"/> interface</para>
    /// </remarks>
    public interface IQueryBus
    {
        /// <summary>
        /// Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>Task which will complete once we've got the result (or something failed, like a query wait timeout).</returns>
        /// <exception cref="ArgumentNullException">query</exception>
        Task<TResult> QueryAsync<TResult>(Query<TResult> query);
    }
}
