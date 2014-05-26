using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{

    public class BusFailedEventArgsTests
    {
        [Fact]
        public void require_that_the_exception_is_included()
        {

            Action actual = () => new BusFailedEventArgs(null);

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void exception_should_Be_assigned_to_property()
        {
            var exception = new Exception();

            var sut = new BusFailedEventArgs(exception);

            sut.Exception.Should().BeSameAs(exception);
        }
    }
}
