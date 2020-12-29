using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Logging;

namespace DotNetCqs.Queues.AdoNet
{
    public class AdoNetMessageQueueSession : IMessageQueueSession
    {
        private readonly IMessageSerializer<string> _messageSerializer;
        private readonly string _queueName;
        private readonly string _tableName;
        private IDbConnection _connection;
        private bool _dequeued;
        private IDbTransaction _transaction;

        public AdoNetMessageQueueSession(string tableName, string queueName, IDbConnection connection,
            IMessageSerializer<string> messageSerializer)
        {
            _messageSerializer = messageSerializer;
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = connection.BeginTransaction();
        }


        public AdoNetMessageQueueSession(string tableName, string queueName, IDbTransaction transaction,
            IMessageSerializer<string> messageSerializer)
        {
            _messageSerializer = messageSerializer;
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _connection = transaction.Connection;
        }

        public Task<Message> Dequeue(TimeSpan suggestedWaitTime)
        {
            EnsureNotDequeued();

            var id = 0;
            AdoNetMessageDto msg;
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = _transaction;
                cmd.CommandText = $"SELECT TOP(1) Id, Body FROM {_tableName} WITH (READPAST) WHERE QueueName = @name";
                cmd.AddParameter("name", _queueName);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return Task.FromResult<Message>(null);

                    id = reader.GetInt32(0);
                    var data = reader.GetString(1);
                    msg = (AdoNetMessageDto) _messageSerializer.Deserialize("Message", data);
                }
            }
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = _transaction;
                cmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";
                cmd.AddParameter("id", id);
                cmd.ExecuteNonQuery();
            }


            msg.ToMessage(_messageSerializer, out var message, out var user);
            msg.Properties["X-AdoNet-Id"] = id.ToString();
            return Task.FromResult(message);
        }

        public async Task<DequeuedMessage> DequeueWithCredentials(TimeSpan suggestedWaitTime)
        {
            EnsureNotDequeued();

            int id;
            string data;
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = _transaction;
                cmd.CommandText = $"SELECT TOP(1) Id, Body FROM {_tableName} WITH (READPAST) WHERE QueueName = @name";
                cmd.AddParameter("name", _queueName);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        await Task.Delay(500);
                        return null;
                    }


                    id = reader.GetInt32(0);
                    data = reader.GetString(1);
                }
            }
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = _transaction;
                cmd.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";
                cmd.AddParameter("id", id);
                cmd.ExecuteNonQuery();
            }

            var msg = (AdoNetMessageDto) _messageSerializer.Deserialize("Message", data);
            msg.ToMessage(_messageSerializer, out var message, out var principal);
            msg.Properties["X-AdoNet-Id"] = id.ToString();
            return new DequeuedMessage(principal, message);
        }

        public Task EnqueueAsync(ClaimsPrincipal user, IReadOnlyCollection<Message> messages)
        {
            foreach (var msg in messages)
            {
                var dto = new AdoNetMessageDto(user, msg, _messageSerializer);
                _messageSerializer.Serialize(dto, out var json, out var contentType);
                InsertMessage(json, contentType);
            }
            return Task.FromResult<object>(null);
        }

        public Task EnqueueAsync(IReadOnlyCollection<Message> messages)
        {
            foreach (var msg in messages)
            {
                var dto = new AdoNetMessageDto(null, msg, _messageSerializer);
                _messageSerializer.Serialize(dto, out var json, out var contentType);

                InsertMessage(json, contentType);
            }
            return Task.FromResult<object>(null);
        }

        public Task EnqueueAsync(ClaimsPrincipal user, Message msg)
        {
            var dto = new AdoNetMessageDto(user, msg, _messageSerializer);
            _messageSerializer.Serialize(dto, out var json, out var contentType);

            InsertMessage(json, contentType);

            return Task.FromResult<object>(null);
        }

        public Task EnqueueAsync(Message msg)
        {
            var dto = new AdoNetMessageDto(null, msg, _messageSerializer);
            _messageSerializer.Serialize(dto, out var json, out var contentType);

            InsertMessage(json, contentType);

            return Task.FromResult<object>(null);
        }

        public Task SaveChanges()
        {
            _transaction.Commit();
            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
            _connection?.Dispose();
            _connection = null;
        }

        private void EnsureNotDequeued()
        {
            if (_dequeued)
                throw new NotSupportedException(
                    "The ADO.NET queue do not support multiple dequeues in the same message scope. It's because we use 'SELECT TOP(1)' internally which means that the same message is returned every time until the internal transaction is comitted.");
            _dequeued = true;
        }

        private void InsertMessage(string json, string typeName)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = _transaction;
                cmd.CommandText =
                    $"INSERT INTO {_tableName} (CreatedAtUtc, QueueName, MessageType, Body) VALUES(GetUtcDate(),  @name, @MessageType, @Body)";
                cmd.AddParameter("name", _queueName);
                cmd.AddParameter("Body", json);
                cmd.AddParameter("MessageType", typeName);
                cmd.ExecuteNonQuery();
            }
        }
    }
}