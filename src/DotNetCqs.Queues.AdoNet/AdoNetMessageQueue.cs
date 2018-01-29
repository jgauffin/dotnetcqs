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

        /// <summary>
        ///     Isolation level to use
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default is unspecified (i.e. let the ADO.NET driver decide)
        /// </para>
        /// </remarks>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        ///     Table which messages are read for
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Name of the queue (to query the correct messages in the table)
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Creates a new session (represents an ADO.NET transaction in which a set of operations is made).
        /// </summary>
        /// <returns></returns>
        public IMessageQueueSession BeginSession()
        {
            var connection = _connectionFactory();
            if (IsolationLevel == IsolationLevel.Unspecified)
                return new AdoNetMessageQueueSession(TableName, Name, connection, _messageSerializer);

            var transaction = connection.BeginTransaction(IsolationLevel);
            return new AdoNetMessageQueueSession(TableName, Name, transaction, _messageSerializer);
        }
    }
}