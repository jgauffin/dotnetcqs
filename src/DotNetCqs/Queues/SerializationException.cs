using System;

namespace DotNetCqs.Queues
{
    public class SerializationException : CqsException
    {
        public SerializationException(string errorMessage, object messageThatFailed) : base(errorMessage)
        {
            MessageThatFailed = messageThatFailed;
        }

        public SerializationException(string errorMessage, object messageThatFailed, Exception inner) : base(
            errorMessage, inner)
        {
            MessageThatFailed = messageThatFailed;
        }


        /// <summary>
        ///     Is either a DTO or raw data depending on if it's the serialization or deserialization that failed.
        /// </summary>
        public object MessageThatFailed { get; }
    }
}