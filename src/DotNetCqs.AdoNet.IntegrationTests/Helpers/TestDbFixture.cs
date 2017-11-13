using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers
{
    public class TestDbFixture : IDisposable
    {
        private readonly List<AdoNetMessageQueueSession> _sessions = new List<AdoNetMessageQueueSession>();
        private List<IDbConnection> _connections = new List<IDbConnection>();

        public TestDbFixture()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("TestSettings.json");
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Dispose()
        {
            foreach (var session in _sessions)
                session.Dispose();
            foreach (var connection in _connections)
            {
                connection.Close();
            }
            _sessions.Clear();
        }

        public Dictionary<string, object> GetFirstRow(string queueName, bool deleteExistingItems = false)
        {
            using (var connnection = OpenConnection(queueName, deleteExistingItems))
            {
                return connnection.QueryRow($"SELECT TOP(1) * FROM MessageQueues WHERE QueueName = '{queueName}'");
            }
        }

        public SqlConnection OpenConnection(string queueName, bool deleteExistingMessages = true)
        {
            var connection = new SqlConnection {ConnectionString = Configuration.GetConnectionString("TestDb")};
            connection.Open();
            _connections.Add(connection);
            if (deleteExistingMessages)
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM MessageQueues WHERE QueueName='{queueName}'";
                    cmd.ExecuteNonQuery();
                }
            return connection;
        }

        public IMessageQueue OpenQueue(string queueName, bool deleteExistingMessages = false)
        {
            return new AdoNetMessageQueue(queueName, () => OpenConnection(queueName, deleteExistingMessages),
                new JsonSerializer())
            {
                TableName = "MessageQueues",
                IsolationLevel = IsolationLevel.Serializable
            };
        }

        public AdoNetMessageQueueSession OpenSession(string queueName, bool deleteExistingMessages = false)
        {
            return (AdoNetMessageQueueSession) OpenQueue(queueName, deleteExistingMessages).BeginSession();
        }

        public void ClearQueue(string queueName)
        {
            OpenConnection(queueName, true).Close();
        }
    }
}