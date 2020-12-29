using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotNetCqs.Logging
{
    public interface ILogFormatter
    {
        public string FormatEntry(LogLevel level, string queueName, Message message, ClaimsPrincipal principal, string logMessage);
    }
}
