using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests
{
    public class QueueListenerTests : IDisposable, IClassFixture<TestDbFixture>
    {
        TestDbFixture _fixture;
        private IMessageQueue _inboundQueue;
        private IHandlerScopeFactory _scopeFactory;
        private QueueListener _queueListener;
        private IHandlerScope _handlerScope;
        private IMessageInvoker _messageInvoker;


        public QueueListenerTests(TestDbFixture fixture)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            LogConfiguration.LogFactory = loggerFactory;

            fixture.ClearQueue("QLInbound");
            _inboundQueue = fixture.OpenQueue("QLInbound");
            _scopeFactory = Substitute.For<IHandlerScopeFactory>();
            _queueListener = new QueueListener(_inboundQueue, fixture.OpenQueue("QLOutbound"), _scopeFactory);

            _handlerScope = Substitute.For<IHandlerScope>();
            _scopeFactory.CreateScope().Returns(_handlerScope);
            _messageInvoker = Substitute.For<IMessageInvoker>();
            _handlerScope.ResolveDependency<IMessageInvoker>().Returns(new[] { _messageInvoker });

            this._fixture = fixture;
        }

        [Fact]
        public async Task Should_remove_poison_messages_from_the_queue()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0,1);
            _messageInvoker.WhenForAnyArgs(x => x.ProcessAsync(null, null))
                .Do(x => throw new InvalidOperationException());
            var token = new CancellationTokenSource();
            var session = _fixture.OpenSession("QLInbound");
            await session.EnqueueAsync(new Message("Hello world"));
            await session.SaveChanges();
            _queueListener.PoisonMessageDetected += (o,e) => semaphore.Release();

            _queueListener.RetryAttempts = new[] {TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10),};
            var t = _queueListener.RunAsync(token.Token).ConfigureAwait(false).GetAwaiter();
            var actual = await semaphore.WaitAsync(10000);


            actual.Should().BeTrue();
            var row = _fixture.GetFirstRow("QLInbound");
            row.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_be_able_to_process_a_message()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
            var token = new CancellationTokenSource();
            var session = _fixture.OpenSession("QLInbound");
            await session.EnqueueAsync(new Message("Hello world"));
            await session.SaveChanges();
            _messageInvoker.WhenForAnyArgs(x => x.ProcessAsync(null, null))
                .Do(x => semaphore.Release());

            _queueListener.RetryAttempts = new[] { TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10), };
            var t = _queueListener.RunAsync(token.Token).ConfigureAwait(false).GetAwaiter();
            var actual = await semaphore.WaitAsync(500);


            actual.Should().BeTrue();
            var row = _fixture.GetFirstRow("QLInbound");
            row.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_only_invoke_handler_once()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
            var token = new CancellationTokenSource();
            var session = _fixture.OpenSession("QLInbound");
            await session.EnqueueAsync(new Message("Hello world"));
            await session.SaveChanges();
            _messageInvoker.WhenForAnyArgs(x => x.ProcessAsync(null, null))
                .Do(x => semaphore.Release());

            _queueListener.RetryAttempts = new[] { TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10), };
            var t = _queueListener.RunAsync(token.Token).ConfigureAwait(false).GetAwaiter();
            var actual = await semaphore.WaitAsync(500);


            actual.Should().BeTrue();
            var row = _fixture.GetFirstRow("QLInbound");
            row.Should().BeEmpty();
            
        }


        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
