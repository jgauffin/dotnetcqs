using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{
    public class CommandHandlerFailedEventArgsTests
    {
        [Fact]
        public void command_should_be_required_in_the_constructor()
        {

            Action actual = () => new CommandHandlerFailedEventArgs(null, new object(), new Exception());

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void handler_should_be_required_in_the_constructor()
        {

            Action actual = () => new CommandHandlerFailedEventArgs(new Command(), null, new Exception());

            actual.ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public void exception_should_be_required_in_the_constructor()
        {

            Action actual = () => new CommandHandlerFailedEventArgs(new Command(), new object(), null);

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void command_Should_be_assigned_to_property()
        {
            var expected = new Command();

            var sut = new CommandHandlerFailedEventArgs(expected, new object(), new Exception());

            sut.Command.Should().BeSameAs(expected);
        }

        [Fact]
        public void handler_Should_be_assigned_to_property()
        {
            var expected = new object();

            var sut = new CommandHandlerFailedEventArgs(new Command(), expected, new Exception());

            sut.Handler.Should().BeSameAs(expected);
        }

        [Fact]
        public void exception_Should_be_assigned_to_property()
        {
            var expected = new Exception();

            var sut = new CommandHandlerFailedEventArgs(new Command(), new object(), expected);

            sut.Exception.Should().BeSameAs(expected);
        }
    }
}
