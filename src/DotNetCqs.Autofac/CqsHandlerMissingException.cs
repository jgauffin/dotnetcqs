using System;
using System.Runtime.Serialization;

namespace DotNetCqs.Autofac
{
    [Serializable]
    public class CqsHandlerMissingException : Exception
    {
    
        public CqsHandlerMissingException(Type type)
            : base("Missing a handler for '" + type.FullName + "'.")
        {
        }

        protected CqsHandlerMissingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}