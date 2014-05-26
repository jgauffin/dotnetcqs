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
    public class ContainerCommandBusTests
    {
        [Fact]
        public async Task executed_job_should_be_stored()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var cmd = new Command();

            var sut = new ContainerCommandBus(storage, container);
            await sut.ExecuteAsync(cmd);

            storage.Received().PushAsync(cmd);
        }

        [Fact]
        public async Task ExecuteJobAsync_should_return_directly_if_there_are_no_more_jobs()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();

            var sut = new ContainerCommandBus(storage, container);
            var result = await sut.ExecuteJobAsync();

            result.Should().BeFalse();
            container.ReceivedCalls().Should().BeEmpty();
        }

        [Fact]
        public void allow_only_one_command_handler()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<ICommandHandler<Command>>();
            var handler2 = Substitute.For<ICommandHandler<Command>>();
            storage.PopCommandAsync().Returns(Task.FromResult(new Command()));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(ICommandHandler<Command>))
                .Returns(new object[] {handler1, handler2});

            var sut = new ContainerCommandBus(storage, container);
            var task = sut.ExecuteJobAsync();
            Action actual = task.Wait;

            actual.ShouldThrow<OnlyOneHandlerAllowedException>();
        }

        [Fact]
        public void a_comamnd_handlr_is_mandatory()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            storage.PopCommandAsync().Returns(Task.FromResult(new Command()));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(ICommandHandler<Command>))
                .Returns(new object[0]);

            var sut = new ContainerCommandBus(storage, container);
            var task = sut.ExecuteJobAsync();
            Action actual = task.Wait;

            actual.ShouldThrow<CqsHandlerMissingException>();
        }

        [Fact]
        public async Task invoke_the_handler_successfully()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<ICommandHandler<Command>>();
            storage.PopCommandAsync().Returns(Task.FromResult(new Command()));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(ICommandHandler<Command>))
                .Returns(new object[] { handler1 });

            var sut = new ContainerCommandBus(storage, container);
            var actual = await sut.ExecuteJobAsync();

            actual.Should().BeTrue();
        }

        [Fact]
        public async Task trigger_failed_event_When_handler_throws_an_Exception()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<ICommandHandler<Command>>();
            var command = new Command();
            handler1
                .When(x => x.ExecuteAsync(command))
                .Do(x => { throw new InvalidCastException(); });
            storage.PopCommandAsync().Returns(Task.FromResult(command));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(ICommandHandler<Command>))
                .Returns(new object[] { handler1 });
            CommandHandlerFailedEventArgs actual = null;

            var sut = new ContainerCommandBus(storage, container);
            sut.HandlerFailed += (sender, args) => actual = args;
            await sut.ExecuteJobAsync();

            actual.Should().NotBeNull();
            actual.Command.Should().BeSameAs(command);
            actual.Handler.Should().Be(handler1);
            actual.Exception.Should().BeOfType<InvalidCastException>();
        }
    }
}
