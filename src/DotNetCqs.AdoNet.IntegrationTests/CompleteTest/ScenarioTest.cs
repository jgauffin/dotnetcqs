using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages;
using DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest
{
    public class ScenarioTest : IDisposable, IClassFixture<TestDbFixture>
    {
        TestDbFixture _fixture;

        public ScenarioTest(TestDbFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact]
        public async Task Should_be_able_to_handle_a_message_flow()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var scopeFactory1 = Substitute.For<IHandlerScopeFactory>();
            var scopeFactory2 = Substitute.For<IHandlerScopeFactory>();
            var scope1 = Substitute.For<IHandlerScope>();
            var scope2 = Substitute.For<IHandlerScope>();
            var upgrade = new LogAdminUpgrades(evt);
            var inboundQueue = _fixture.OpenQueue("inbound", true);
            var outboundQueue = _fixture.OpenQueue("outbound", true);
            scopeFactory1.CreateScope().Returns(scope1);
            scopeFactory2.CreateScope().Returns(scope2);
            scope1.ResolveDependency<IMessageInvoker>().Returns(new[] { new MessageInvoker(scope1) });
            scope2.ResolveDependency<IMessageInvoker>().Returns(new[] { new MessageInvoker(scope2) });
            scope1.Create(typeof(IMessageHandler<ActivateUser>)).Returns(new[] { new ActivateUserHandler() });
            scope2.Create(typeof(IMessageHandler<UserActivated>)).Returns(new[] { new UpgradeToAdminHandler() });
            scope2.Create(typeof(IMessageHandler<UserBecameAdmin>)).Returns(new[] { upgrade });
            var token = new CancellationTokenSource();
            using (var session = inboundQueue.BeginSession())
            {
                await session.EnqueueAsync(new Message(new ActivateUser()));
                await session.SaveChanges();
            }
            var listener1 = new QueueListener(inboundQueue, outboundQueue, scopeFactory1);
            listener1.Logger = (level, queue, msg) => Console.WriteLine($"{level} {queue} {msg}");
            var listener2 = new QueueListener(outboundQueue, outboundQueue, scopeFactory2);
            listener2.Logger = (level, queue, msg) => Console.WriteLine($"{level} {queue} {msg}");

            var t1 = listener1.RunAsync(token.Token);
            var t2 = listener2.RunAsync(token.Token);


            token.Cancel();
            await Task.WhenAll(t1, t2);
            evt.WaitOne(500).Should().BeTrue();
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
