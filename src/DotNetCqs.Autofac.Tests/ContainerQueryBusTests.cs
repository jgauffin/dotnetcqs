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
    public class ContainerQueryBusTests
    {
        [Fact]
        public void allow_only_one_query_handler()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IQueryHandler<Query<string>, string>>();
            var handler2 = Substitute.For<IQueryHandler<Query<string>, string>>();
            var query = Substitute.For<Query<string>>();
            storage.PopQueryAsync().Returns(Task.FromResult((IQuery)query));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(string)))
                .Returns(new object[] { handler1, handler2 });

            var sut = new ContainerQueryBus(container);
            var task = sut.QueryAsync(query);
            Action actual = task.Wait;

            actual.ShouldThrow<OnlyOneHandlerAllowedException>();
        }

        [Fact]
        public void a_query_handler_is_mandatory()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var query = Substitute.For<Query<string>>();
            storage.PopQueryAsync().Returns(Task.FromResult((IQuery)query));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IQueryHandler<Query<string>, string>))
                .Returns(new object[0]);

            var sut = new ContainerQueryBus(container);
            var task = sut.QueryAsync(query);
            Action actual = task.Wait;

            actual.ShouldThrow<CqsHandlerMissingException>();
        }

        [Fact]
        public async Task invoke_the_handler_successfully()
        {
            var storage = Substitute.For<IFileStorage>();
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IQueryHandler<Query<string>, string>>();
            var query = Substitute.For<Query<string>>();
            handler1.ExecuteAsync(query).Returns(Task.FromResult("Hello world"));
            storage.PopQueryAsync().Returns(Task.FromResult((IQuery)query));
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(string)))
                .Returns(new object[] { handler1 });

            var sut = new ContainerQueryBus(container);
            var actual = await sut.QueryAsync(query);

            actual.Should().Be("Hello world");
        }

        [Fact]
        public async Task do_not_catch_handler_Exceptions()
        {
            var container = Substitute.For<IContainer>();
            var scope = Substitute.For<IContainerScope>();
            var handler1 = Substitute.For<IQueryHandler<Query<string>, string>>();
            var query = Substitute.For<Query<string>>();
            handler1
                .When(x => x.ExecuteAsync(query))
                .Do(x => { throw new InvalidCastException(); });
            container.CreateScope().Returns(scope);
            scope.ResolveAll(typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(string)))
                .Returns(new object[] { handler1 });

            var sut = new ContainerQueryBus(container);
            try
            {
                sut.QueryAsync(query).Wait();
                Assert.False(true, "Query did not fail");
            }
            catch (AggregateException exception)
            {
                scope.Received().ResolveAll(typeof (IQueryHandler<,>).MakeGenericType(query.GetType(), typeof (string)));
                exception.InnerException.Should().BeOfType<InvalidCastException>();
            }



        }
    }
}
