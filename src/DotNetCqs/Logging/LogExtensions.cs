using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace DotNetCqs.Logging
{
    /// <summary>
    ///     Extensions used to format log messages the same way in all places.
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        ///     Debug log message
        /// </summary>
        /// <param name="instance">logger</param>
        /// <param name="queueName">Queue logging</param>
        /// <param name="logMessage">Message to write to the log</param>
        /// <param name="principal">Principal (if any)</param>
        /// <param name="messageBeingProcessed">Message being processed (if any)</param>
        public static void Debug(this ILogger instance, string queueName, string logMessage,
            ClaimsPrincipal principal = null, Message messageBeingProcessed = null)
        {
            instance.LogDebug(Format(LogLevel.Debug, queueName, logMessage, principal, messageBeingProcessed));
        }

        /// <summary>
        ///     Debug log message
        /// </summary>
        /// <param name="instance">logger</param>
        /// <param name="queueName">Queue logging</param>
        /// <param name="logMessage">Message to write to the log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="principal">Principal (if any)</param>
        /// <param name="messageBeingProcessed">Message being processed (if any)</param>
        public static void Error(this ILogger instance, string queueName, string logMessage, Exception exception,
            ClaimsPrincipal principal = null, Message messageBeingProcessed = null)
        {
            instance.LogError(exception,
                Format(LogLevel.Error, queueName, logMessage, principal, messageBeingProcessed));
        }

        /// <summary>
        ///     Debug log message
        /// </summary>
        /// <param name="instance">logger</param>
        /// <param name="queueName">Queue logging</param>
        /// <param name="logMessage">Message to write to the log</param>
        /// <param name="principal">Principal (if any)</param>
        /// <param name="messageBeingProcessed">Message being processed (if any)</param>
        public static void Info(this ILogger instance, string queueName, string logMessage,
            ClaimsPrincipal principal = null, Message messageBeingProcessed = null)
        {
            instance.LogDebug(Format(LogLevel.Information, queueName, logMessage, principal, messageBeingProcessed));
        }

        /// <summary>
        ///     Convert a principal to a log friendly string.
        /// </summary>
        /// <param name="principal">Principal</param>
        /// <returns></returns>
        public static string ToLogString(this ClaimsPrincipal principal)
        {
            if (principal?.Identity == null)
                return "null";
            if (!principal.Identity.IsAuthenticated)
                return "Anonymous";

            var claims = "";
            foreach (var claim in principal.Claims)
            {
                var type = claim.Type;
                if (type.StartsWith("http://schemas.microsoft.com/ws/2008/06/identity/claims"))
                    type = type.Remove(0, "http://schemas.microsoft.com/ws/2008/06/identity/claims/".Length);
                else if (type.StartsWith("http://schemas.xmlsoap.org/ws/2005/05/identity/claims"))
                    type = type.Remove(0, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/".Length);

                claims += $"{type}={claim.Value},";
            }

            claims = claims.Remove(claims.Length - 1, 1);
            return claims;
        }

        /// <summary>
        ///     Debug log message
        /// </summary>
        /// <param name="instance">logger</param>
        /// <param name="queueName">Queue logging</param>
        /// <param name="logMessage">Message to write to the log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="principal">Principal (if any)</param>
        /// <param name="messageBeingProcessed">Message being processed (if any)</param>
        public static void Warning(this ILogger instance, string queueName, string logMessage,
            Exception exception = null,
            ClaimsPrincipal principal = null, Message messageBeingProcessed = null)
        {
            if (exception != null)
                instance.LogWarning(exception,
                    Format(LogLevel.Warning, queueName, logMessage, principal, messageBeingProcessed));
            else
                instance.LogWarning(Format(LogLevel.Warning, queueName, logMessage, principal, messageBeingProcessed));
        }

        /// <summary>
        ///     Debug log message
        /// </summary>
        /// <param name="level">Level used for the log</param>
        /// <param name="queueName">Queue logging</param>
        /// <param name="logMessage">Message to write to the log</param>
        /// <param name="principal">Principal (if any)</param>
        /// <param name="messageBeingProcessed">Message being processed (if any)</param>
        private static string Format(LogLevel level, string queueName, string logMessage,
            ClaimsPrincipal principal = null, Message messageBeingProcessed = null)
        {
            if (LogConfiguration.Formatter != null)
                return LogConfiguration.Formatter.FormatEntry(level, queueName, messageBeingProcessed, principal,
                    logMessage);

            return level == LogLevel.Debug
                ? $"[queue: {queueName}, principal: {principal.ToLogString()}] {logMessage}"
                : $"[queue: {queueName}, principal: {principal.ToLogString()}, msgType: {messageBeingProcessed?.Body.GetType().FullName}] {logMessage}";
        }
    }
}