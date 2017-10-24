namespace DotNetCqs.DependencyInjection
{
    /// <summary>
    ///     Create a new scope
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Invoked when we are about to process a new message.
    ///     </para>
    /// </remarks>
    public interface IHandlerScopeFactory
    {
        /// <summary>
        ///     Create a new scope.
        /// </summary>
        /// <returns>scope</returns>
        IHandlerScope CreateScope();
    }
}