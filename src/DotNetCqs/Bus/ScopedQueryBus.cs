using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.MessageProcessor;

namespace DotNetCqs.Bus
{
    /// <summary>
    ///     Executes query handlers directly without queuing messages.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Do note that there may only be one query handler per query. Otherwise the result is non-deterministic.
    ///     </para>
    /// </remarks>
    public class ScopedQueryBus : IQueryBus
    {
        private readonly IMessageInvoker _messageInvoker;

        /// <summary>
        ///     Creates new instance of <see cref="ScopedQueryBus" />.
        /// </summary>
        /// <param name="messageInvoker">Used to execute queries</param>
        /// <exception cref="ArgumentNullException">messageInvoker</exception>
        public ScopedQueryBus(IMessageInvoker messageInvoker)
        {
            _messageInvoker = messageInvoker ?? throw new ArgumentNullException(nameof(messageInvoker));
        }

        /// <summary>
        ///     Execute queries
        /// </summary>
        /// <typeparam name="TResult">Query result type</typeparam>
        /// <param name="principal">User executing the query</param>
        /// <param name="query">Query to execute</param>
        /// <returns>result</returns>
        /// <exception cref="ArgumentNullException">principal;query</exception>
        /// <exception cref="OnlyOneQueryHandlerAllowedException">If multiple handlers have been registered for the query</exception>
        /// <exception cref="NoHandlerRegisteredException">A handler have not been registered for the given query</exception>
        public async Task<TResult> QueryAsync<TResult>(ClaimsPrincipal principal, Query<TResult> query)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (query == null) throw new ArgumentNullException(nameof(query));
            var ctx = new ExecuteQueriesInvocationContext(principal, _messageInvoker, "Direct");
            return await ctx.QueryAsync(query);
        }

        /// <summary>
        ///     Execute query
        /// </summary>
        /// <typeparam name="TResult">Query result type</typeparam>
        /// <param name="query">Query to execute</param>
        /// <returns>result</returns>
        /// <exception cref="ArgumentNullException">query</exception>
        /// <exception cref="OnlyOneQueryHandlerAllowedException">If multiple handlers have been registered for the query</exception>
        /// <exception cref="NoHandlerRegisteredException">A handler have not been registered for the given query</exception>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            var ctx = new ExecuteQueriesInvocationContext(null, _messageInvoker, "Direct");
            return await ctx.QueryAsync(query);
        }
    }
}