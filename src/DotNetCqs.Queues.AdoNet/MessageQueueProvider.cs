using System;
using System.Data;

namespace DotNetCqs.Queues.AdoNet
{
    public class AdoNetMessageQueueProvider : IMessageQueueProvider
    {
        private readonly Func<IDbConnection> _connection;
        private readonly IMessageSerializer<string> _serializer;

        public AdoNetMessageQueueProvider(Func<IDbConnection> connection, IMessageSerializer<string> serializer)
        {
            _connection = connection;
            _serializer = serializer;
            TableName = "MessageQueue";
        }

        /// <summary>
        /// </summary>
        public string TableName { get; set; }

        public IMessageQueue Open(string queueName)
        {
            return new AdoNetMessageQueue(queueName, _connection, _serializer)
            {
                TableName = "MessageQueue"
            };
        }
    }
}