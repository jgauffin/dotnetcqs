using DotNetCqs.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetCqs
{
    /// <summary>
    /// Used to adapt logging behavior.
    /// </summary>
    public class LogConfiguration
    {
        /// <summary>
        /// Specify which factory to use.
        /// </summary>
        public static ILoggerFactory LogFactory  = NullLoggerFactory.Instance;

        /// <summary>
        /// Used to format log entries based on given data and log level.
        /// </summary>
        public static ILogFormatter Formatter;
    }
}
