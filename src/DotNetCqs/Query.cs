using System;

namespace DotNetCqs
{
    /// <summary>
    ///     Query base class.
    /// </summary>
    /// <typeparam name="TResult">Type of result that the query will return</typeparam>
    /// <remarks>
    ///     <para>
    ///         Queries may not change the application state. They are only used to fetch pre-generated data. And by following
    ///         that principle we can introduce caching later on.
    ///     </para>
    ///     <para>
    ///         Uses <see cref="GuidFactory" /> to assign the <see cref="QueryId" />.
    ///     </para>
    /// </remarks>
    public class Query<TResult> : IQuery
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Query{TResult}" /> class.
        /// </summary>
        public Query()
        {
            QueryId = GuidFactory.Create();
        }

        /// <summary>
        ///     Gets unique identifier for this query.
        /// </summary>
        public Guid QueryId { get; private set; }
    }
}