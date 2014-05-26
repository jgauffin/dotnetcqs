using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{
    public class EventHandlerFailedEventArgsTests
    {
        [Fact]
        public void verify_Assignment()
        {
            var evt = Substitute.For<ApplicationEvent>();
            var failures = new List<HandlerFailure>();

            var sut = new EventHandlerFailedEventArgs(evt, failures, 10);

            sut.ApplicationEvent.Should().BeSameAs(evt);
            sut.Failures.Should().BeEquivalentTo(failures);
            sut.HandlerCount.Should().Be(10);
        }
    }
}
