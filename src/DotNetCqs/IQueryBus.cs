using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    ///     Bus intended to send await queries and wait for their result
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Queries are dangerous in messaging systems if they are invoked within the system and
    ///         on the same resources as in command handlers.
    ///     </para>
    /// </remarks>
    public interface IQueryBus
    {
        /// <summary>
        ///     Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="principal">logged in principal</param>
        /// <param name="query">QueryAsync to execute.</param>
        /// <returns>Task which will complete once we've got the result (or something failed, like a query wait timeout).</returns>
        /// <exception cref="ArgumentNullException">principal;query</exception>
        /// <exception cref="OnlyOneQueryHandlerAllowedException">
        ///     If more than one query handlers have been registered for the same
        ///     query.
        /// </exception>
        /// <exception cref="NoHandlerRegisteredException">A handler have not been registered for the given query</exception>
        Task<TResult> QueryAsync<TResult>(ClaimsPrincipal principal, Query<TResult> query);

        /// <summary>
        ///     Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="query">QueryAsync to execute.</param>
        /// <returns>Task which will complete once we've got the result (or something failed, like a query wait timeout).</returns>
        /// <exception cref="ArgumentNullException">query</exception>
        /// <exception cref="OnlyOneQueryHandlerAllowedException">
        ///     If more than one query handlers have been registered for the same
        ///     query.
        /// </exception>
        /// <exception cref="NoHandlerRegisteredException">A handler have not been registered for the given query</exception>
        Task<TResult> QueryAsync<TResult>(Query<TResult> query);
    }
}