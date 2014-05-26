using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{
    public class ContainerEventBusTests
    {
        [Fact]
        public async Task executed_job_should_be_stored()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var appEvent = new ApplicationEvent();

            var sut = new ContainerEventBus(storage, container);
            await sut.PublishAsync(appEvent);

            storage.Received().PushAsync(appEvent);
        }

        [Fact]
        public async Task PublishAsync_should_return_directly_if_there_are_no_more_jobs()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();

            var sut = new ContainerEventBus(storage, container);
            await sut.PublishAsync(Substitute.For<ApplicationEvent>());

            container.ReceivedCalls().Should().BeEmpty();
        }

        [Fact]
        public void make_sure_That_both_handlers_are_invoked()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IApplicationEventSubscriber<ApplicationEvent>>();
            var handler2 = Substitute.For<IApplicationEventSubscriber<ApplicationEvent>>();
            storage.PopEventAsync().Returns(Task.FromResult(new ApplicationEvent()));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IApplicationEventSubscriber<ApplicationEvent>))
                .Returns(new object[] {handler1, handler2});

            var sut = new ContainerEventBus(storage, container);
            var task = sut.PublishAsync(Substitute.For<ApplicationEvent>());
            task.Wait();
            sut.ExecuteJobAsync().Wait();

            handler1.ReceivedWithAnyArgs().HandleAsync(null);
            handler2.ReceivedWithAnyArgs().HandleAsync(null);
        }

        [Fact]
        public void a_comamnd_handlr_is_mandatory()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            storage.PopEventAsync().Returns(Task.FromResult(new ApplicationEvent()));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IApplicationEventSubscriber<ApplicationEvent>))
                .Returns(new object[0]);

            var sut = new ContainerEventBus(storage, container);
            var task = sut.PublishAsync(Substitute.For<ApplicationEvent>());
            task.Wait();

        }

        [Fact]
        public async Task invoke_the_handler_successfully()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IApplicationEventSubscriber<ApplicationEvent>>();
            storage.PopEventAsync().Returns(Task.FromResult(new ApplicationEvent()));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IApplicationEventSubscriber<ApplicationEvent>))
                .Returns(new object[] { handler1 });

            var sut = new ContainerEventBus(storage, container);
            await sut.PublishAsync(Substitute.For<ApplicationEvent>());
            sut.ExecuteJobAsync().Wait();

            handler1.ReceivedWithAnyArgs().HandleAsync(null);
        }

        [Fact]
        public async Task trigger_failed_event_When_handler_throws_an_Exception()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IApplicationEventSubscriber<ApplicationEvent>>();
            var ApplicationEvent = new ApplicationEvent();
            handler1
                .When(x => x.HandleAsync(ApplicationEvent))
                .Do(x => { throw new InvalidCastException(); });
            storage.PopEventAsync().Returns(Task.FromResult(ApplicationEvent));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IApplicationEventSubscriber<ApplicationEvent>))
                .Returns(new object[] { handler1 });
            EventHandlerFailedEventArgs actual = null;

            var sut = new ContainerEventBus(storage, container);
            sut.HandlerFailed += (sender, args) => actual = args;
            await sut.PublishAsync(Substitute.For<ApplicationEvent>());
            await sut.ExecuteJobAsync();

            actual.Should().NotBeNull();
            actual.ApplicationEvent.Should().BeSameAs(ApplicationEvent);
            actual.HandlerCount.Should().Be(1);
            actual.Failures[0].Handler.Should().Be(handler1);
            actual.Failures[0].Exception.Should().BeOfType<InvalidCastException>();
        }

        [Fact]
        public async Task make_sure_that_the_Second_handler_is_invoked_if_the_first_fails()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IApplicationEventSubscriber<ApplicationEvent>>();
            var handler2 = Substitute.For<IApplicationEventSubscriber<ApplicationEvent>>();
            var ApplicationEvent = new ApplicationEvent();
            handler1
                .When(x => x.HandleAsync(ApplicationEvent))
                .Do(x => { throw new InvalidCastException(); });
            storage.PopEventAsync().Returns(Task.FromResult(ApplicationEvent));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IApplicationEventSubscriber<ApplicationEvent>))
                .Returns(new object[] { handler1, handler2 });
            EventHandlerFailedEventArgs actual = null;

            var sut = new ContainerEventBus(storage, container);
            sut.HandlerFailed += (sender, args) => actual = args;
            await sut.PublishAsync(Substitute.For<ApplicationEvent>());
            await sut.ExecuteJobAsync();

            handler2.ReceivedWithAnyArgs().HandleAsync(null);
        }
    }
}
