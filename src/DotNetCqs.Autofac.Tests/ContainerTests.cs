using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using DotNetCqs.Autofac.Tests.TestSubjects;
using FluentAssertions;
using Xunit;

namespace DotNetCqs.Autofac.Tests
{
    public class ContainerTests
    {
        private IContainer _container;

        public ContainerTests()
        {
            var cb = new ContainerBuilder();
            cb.RegisterMessageHandlers(Assembly.GetExecutingAssembly());
            cb.RegisterQueryHandlers(Assembly.GetExecutingAssembly());
            cb.RegisterType<Type1>();
            _container = cb.Build();
        }

        [Fact]
        public void Should_be_able_to_resolve_a_queryHandler()
        {
            var sut = new AutofacHandlerScopeFactory(_container);

            List<object> handlers;
            using (var scope = sut.CreateScope())
            {
                handlers = scope.Create(typeof(IQueryHandler<Type2, Type2Result>)).ToList();
            }

            handlers.Count.Should().Be(1);
            handlers[0].Should().BeOfType<Type2Handler>();
        }

        [Fact]
        public void Should_be_able_to_resolve_messageHandlers()
        {
            var sut = new AutofacHandlerScopeFactory(_container);

            List<object> handlers;
            using (var scope = sut.CreateScope())
            {
                handlers = scope.Create(typeof(IMessageHandler<Type1>)).ToList();
            }

            handlers.Count.Should().Be(2);
            handlers[0].Should().BeOfType<Type1Handler>();
            handlers[1].Should().BeOfType<Type1HandlerB>();
        }

        [Fact]
        public void Should_be_able_to_resolve_dependencies()
        {
            var sut = new AutofacHandlerScopeFactory(_container);

            List<Type1> types;
            using (var scope = sut.CreateScope())
            {
                types = scope.ResolveDependency<Type1>().ToList();
            }

            types[0].Should().NotBeNull();
        }
    }
}
