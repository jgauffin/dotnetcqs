namespace DotNetCqs
{
    /// <summary>
    ///     Queries requires that exactly one handler is activated per query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Other options would require that the library (implementors) should assume how to merge results form different
    ///         queries or how long the query invoker should wait before returning the so far received results.
    ///     </para>
    /// </remarks>
    public class OnlyOneQueryHandlerAllowedException : CqsException
    {
        public OnlyOneQueryHandlerAllowedException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}