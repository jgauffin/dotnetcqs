using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Logging
{
    public class LogFactory
    {
        internal static LogFactory Instance = new LogFactory();

        protected LogFactory()
        {

        }

        public static void Assign(LogFactory factory)
        {
            Instance = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public virtual ILogger GetLogger(Type type)
        {
            return new NullLogger();
        }


    }
}
