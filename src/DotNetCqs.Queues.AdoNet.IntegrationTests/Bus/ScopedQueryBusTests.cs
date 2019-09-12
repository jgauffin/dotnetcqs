using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs.Bus;
using DotNetCqs.DependencyInjection;
using DotNetCqs.DependencyInjection.Microsoft;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Handlers;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Messages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.Bus
{
    public class ScopedQueryBusTests
    {
        [Fact]
        public async Task Should_be_able_To_query_and_get_a_result()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IQueryHandler<FindUser, FindUserResult>, FindUserHandler>()
                .BuildServiceProvider();
            var scopeFactory = new MicrosoftHandlerScopeFactory(serviceProvider);
            FindUserResult actual;

            using (var scope = scopeFactory.CreateScope())
            {
                var invoker = new MessageInvoker(scope);
                var bus = new ScopedQueryBus(invoker);
                actual = await bus.QueryAsync(new FindUser());

            }

            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_be_able_To_query_and_get_a_NULL_result()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IQueryHandler<FindUser, FindUserResult>, FindUserHandler>()
                .BuildServiceProvider();
            var scopeFactory = new MicrosoftHandlerScopeFactory(serviceProvider);
            FindUserResult actual;

            using (var scope = scopeFactory.CreateScope())
            {
                var invoker = new MessageInvoker(scope);
                var bus = new ScopedQueryBus(invoker);
                actual = await bus.QueryAsync(new FindUser(){Id = 1});

            }

            actual.Should().BeNull();
        }
    }
}
