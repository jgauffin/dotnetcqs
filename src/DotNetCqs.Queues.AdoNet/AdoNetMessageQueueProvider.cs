using System;
using System.Data;

namespace DotNetCqs.Queues.AdoNet
{
    /// <summary>
    ///     Configures and creates queue instances
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class do not open connections, it merely tells the <see cref="AdoNetMessageQueue" /> class which
    ///         configuration it should use. (This class is more like a builder.)
    ///     </para>
    /// </remarks>
    public class AdoNetMessageQueueProvider : IMessageQueueProvider
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly IMessageSerializer<string> _serializer;

        public AdoNetMessageQueueProvider(Func<IDbConnection> connectionFactory, IMessageSerializer<string> serializer)
        {
            _connectionFactory = connectionFactory;
            _serializer = serializer;
            TableName = "MessageQueue";
            IsolationLevel = IsolationLevel.Unspecified;
        }

        /// <summary>
        ///     Isolation level for the transactions used to read/write from the queue.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default is unspecified (i.e. let the ADO.NET driver decide)
        ///     </para>
        /// </remarks>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        ///     Table to find messages in.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Open queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public virtual IMessageQueue Open(string queueName)
        {
            var queue = new AdoNetMessageQueue(queueName, _connectionFactory, _serializer)
            {
                TableName = "MessageQueue"
            };
            if (IsolationLevel != IsolationLevel.Unspecified)
                queue.IsolationLevel = IsolationLevel;

            return queue;
        }
    }
}