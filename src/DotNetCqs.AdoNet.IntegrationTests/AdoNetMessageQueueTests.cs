using System;
using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests
{
    public class AdoNetMessageQueueTests : IDisposable, IClassFixture<TestDbFixture>
    {
        TestDbFixture _fixture;

        public AdoNetMessageQueueTests(TestDbFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact]
        public async Task Should_be_able_to_queue_message()
        {
            var session = _fixture.OpenSession("qm1");
            await session.EnqueueAsync(new Message("Hello world"));
            await session.SaveChanges();

            var row = _fixture.GetFirstRow("qm1");
            row.Should().NotBeEmpty();
            row["QueueName"].Should().Be("myQueue");
        }

        [Fact]
        public async Task Should_be_able_to_dequeue_message()
        {
            var session = _fixture.OpenSession("qm2");
            await session.EnqueueAsync(new Message("Hello world"));
            await session.SaveChanges();

            var msg = await session.Dequeue(TimeSpan.FromSeconds(1));

            var row = _fixture.GetFirstRow("qm2");
            row.Should().BeEmpty();
            msg.Body.Should().Be("Hello world");
        }


        [Fact]
        public async Task Should_be_able_to_open_two_transactions_on_same_Table_To_simulate_the_usage_of_the_same_table_for_inbound_and_outbound_queues()
        {
            var session = _fixture.OpenSession("qm3");
            using (var session1 = _fixture.OpenSession("qm3"))
            {
                await session1.EnqueueAsync(new Message("Hello world"));
                await session1.SaveChanges();
            }

            //var msg = await session.Dequeue(TimeSpan.FromSeconds(1));
            using (var mySession = _fixture.OpenSession("qm3"))
            {
                await mySession.EnqueueAsync(new Message("Hello world"));
                await mySession.SaveChanges();
            }


            var row = _fixture.GetFirstRow("qm1");
            row.Should().BeEmpty();
            //msg.Body.Should().Be("Hello world");
        }

        public void Dispose()
        {
            _fixture?.Dispose();
            _fixture = null;
        }
    }
}
