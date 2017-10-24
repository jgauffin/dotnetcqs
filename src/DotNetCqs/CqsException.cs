using System;

namespace DotNetCqs
{
    /// <summary>
    ///     All defined exceptions in this library inherits this one.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The exceptions are typically due to configuration errors.
    ///     </para>
    /// </remarks>
    public class CqsException : Exception
    {
        protected CqsException(string errorMessage) : base(errorMessage)
        {
        }

        protected CqsException(string errorMessage, Exception inner) : base(errorMessage, inner)
        {
        }
    }
}