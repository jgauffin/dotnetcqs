﻿using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    ///     Used to execute the actual query (and deliver a result)
    /// </summary>
    /// <typeparam name="TQuery">Query to execute</typeparam>
    /// <typeparam name="TResult">Type of result which has been requested.</typeparam>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : Query<TResult>
    {
        /// <summary>
        ///     Method used to execute the query
        /// </summary>
        /// <param name="context">Contextual actions and information</param>
        /// <param name="query">Query to execute.</param>
        /// <returns>Task which will contain the result once completed.</returns>
        Task<TResult> HandleAsync(IMessageContext context, TQuery query);
    }
}