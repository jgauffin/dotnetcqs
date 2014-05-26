using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{
    public class HandlerFailureTests
    {
        [Fact]
        public void FactMethodName()
        {
            var handler = new object();
            var exception = new Exception();

            var sut = new HandlerFailure(handler, exception);

            sut.Handler.Should().BeSameAs(handler);
            sut.Exception.Should().BeSameAs(exception);
        }
    }
}
