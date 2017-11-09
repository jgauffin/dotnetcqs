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
            IsolationLevel = IsolationLevel.Unspecified;
        }

        public IsolationLevel IsolationLevel { get; set; }

        public string TableName { get; set; }

        public string Name { get; }

        public IMessageQueueSession BeginSession()
        {
            var connection = _connectionFactory();
            if (IsolationLevel == IsolationLevel.Unspecified)
                return new AdoNetMessageQueueSession(TableName, Name, connection, _messageSerializer);

            var transaction = _connectionFactory().BeginTransaction(IsolationLevel);
            return new AdoNetMessageQueueSession(TableName, Name, transaction, _messageSerializer);
        }
    }
}