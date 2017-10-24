using System;
using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests
{
    public class AdoNetMessageQueueTests : IDisposable, IClassFixture<DatabaseFixture>
    {
        DatabaseFixture _fixture;
        private IMessageQueueSession _session;

        public AdoNetMessageQueueTests(DatabaseFixture fixture)
        {
            this._fixture = fixture;
            var sut = new AdoNetMessageQueue("myQueue", () => _fixture.Connection, new JsonSerializer())
            {
                TableName = "#" + _fixture.TableName
            };
            _session = sut.BeginSession();
        }

        [Fact]
        public async Task Should_be_able_to_queue_message()
        {

            await _session.EnqueueAsync(new Message("Hello world"));
            await _session.SaveChanges();

            var row = _fixture.Connection.QueryRow($"SELECT * FROM #{_fixture.TableName}");
            row.Should().NotBeEmpty();
            row["QueueName"].Should().Be("myQueue");
        }

        [Fact]
        public async Task Should_be_able_to_dequeue_message()
        {
            await _session.EnqueueAsync(new Message("Hello world"));
            await _session.SaveChanges();

            var msg = await _session.Dequeue(TimeSpan.FromSeconds(1));

            var row = _fixture.Connection.QueryRow($"SELECT * FROM #{_fixture.TableName}");
            row.Should().BeEmpty();
            msg.Body.Should().Be("Hello world");
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
