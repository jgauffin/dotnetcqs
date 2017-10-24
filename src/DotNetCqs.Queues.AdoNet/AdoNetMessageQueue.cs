using System;
using System.Data;

namespace DotNetCqs.Queues.AdoNet
{
    public class AdoNetMessageQueue : IMessageQueue
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly IMessageSerializer<string> _messageSerializer;


        public AdoNetMessageQueue(string queueName, Func<IDbConnection> connectionFactory,
            IMessageSerializer<string> messageSerializer)
        {
            Name = queueName;
            _connectionFactory = connectionFactory;
            _messageSerializer = messageSerializer;
            TableName = "MessageQueue";
        }

        public string TableName { get; set; }

        public string Name { get; }

        public IMessageQueueSession BeginSession()
        {
            var connection = _connectionFactory();
            return new AdoNetMessageQueueSession(TableName, Name, connection, _messageSerializer);
        }
    }
}