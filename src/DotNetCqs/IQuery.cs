using System;

namespace DotNetCqs
{
    /// <summary>
    ///     A request to fetch information from the data source.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It's highly recommended that you create queries per use case and not per data source design. For instance you
    ///         can
    ///         have a query looking like this:
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// public class FetchTasksForFirstPage : Query<FirstPageTask[]>
    /// {
    ///     public FetchTasksForFirstPage(Guid userId)
    ///     {
    ///         if (userId == Guid.Empty) throw new ArgumentNullException("userId");
    /// 
    ///         UserId = userId;
    ///     }
    /// 
    ///     public Guid UserId { get; private set; }
    /// }
    /// ]]>
    /// </code>
    ///     <para>
    ///         With the following result class:
    ///     </para>
    ///     <example>
    /// <code>
    /// public class FirstPageTask
    /// {
    ///     public string CreatorUserId { get; set; }
    ///     public string CreatorName { get; set; }
    ///     public string TaskId { get; set; }
    ///     public string TaskName { get; set; }
    ///     public int Importance { get; set; }
    ///     public string UpdaterUserId { get; set; }
    ///     public string UpdaterName { get; set; }
    /// }
    /// </code>
    ///     </example>
    ///     <para>
    ///         That result is actually a join between three tables.
    ///     </para>
    ///     <para>
    ///         By designing your queries like that you make it a lot easier to scale your application at a later point.
    ///         Because now you have totally abstracted away the data source. that
    ///         means that you in the future your move from simple joins to views or stored procedures. You could even start to
    ///         create a read model (i.e. a separate database specialized for reads).
    ///     </para>
    ///     <para>
    ///         Hence my recommendation is: Create a query for every specific use case, and always try to flatten the result.
    ///         It makes it so much easier to adapt the backend to the *current* requirements.
    ///     </para>
    /// </remarks>
    public interface IQuery
    {
        /// <summary>
        ///     Query identity. Required to identify queries and their replies (as a query can be used in inter process
        ///     documentation).
        /// </summary>
        Guid QueryId { get; }
    }
}