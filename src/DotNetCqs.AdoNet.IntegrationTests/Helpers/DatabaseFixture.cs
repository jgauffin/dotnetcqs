using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers
{
    public class DatabaseFixture : IDisposable
    {
        private readonly SqlConnection _connection;

        public DatabaseFixture()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("TestSettings.json");
            Configuration = builder.Build();

            TableName = "Test_" + Guid.NewGuid().ToString("N").Substring(0, 5);

            _connection = new SqlConnection();
            Connection.ConnectionString = Configuration.GetConnectionString("TestDb");
            if (Connection.ConnectionString == null)
                throw new InvalidOperationException("Did not find connectionString 'TestDb' in your TestSettings.json");

            Connection.Open();

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = $@"CREATE TABLE #{TableName} ( 
                                        Id int not null identity primary key,
                                        QueueName varchar(40) not null,
                                        CreatedAtUtc datetime not null,
                                        MessageType varchar(512) not null,
                                        Body nvarchar(MAX) not null
                                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public IConfigurationRoot Configuration { get; private set; }

        public string TableName { get; private set; }

        public SqlConnection Connection
        {
            get { return _connection; }
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}