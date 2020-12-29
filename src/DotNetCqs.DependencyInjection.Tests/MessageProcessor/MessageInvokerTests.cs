using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNetCqs.Tests.MessageProcessor
{
    public class MessageInvokerTests
    {
        [Fact]
        public async Task
            Should_be_able_to_reply_with_null_since_that_indicates_that_the_requested_resource_is_not_found()
        {
            var scope = Substitute.For<IHandlerScope>();
            var handler = new MyHandler();
            var msg = new Message(new MyQuery());
            var sut = new MessageInvoker(scope);
            var queryContext = new ExecuteQueriesInvocationContext(new ClaimsPrincipal(), sut, "Direct");
            scope.Create(typeof(IQueryHandler<MyQuery, string>)).Returns(new object[] {handler});

            await sut.ProcessAsync(queryContext, msg);

            queryContext.Replies.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Should_be_able_to_reply_with_a_message()
        {
            var scope = Substitute.For<IHandlerScope>();
            var handler = new MyHandler {Result = "Word!"};
            var msg = new Message(new MyQuery());
            var sut = new MessageInvoker(scope);
            var queryContext = new ExecuteQueriesInvocationContext(new ClaimsPrincipal(), sut, "Direct");
            scope.Create(typeof(IQueryHandler<MyQuery, string>)).Returns(new object[] {handler});

            await sut.ProcessAsync(queryContext, msg);

            queryContext.Replies[0].Body.Should().Be("Word!");
        }
    }
}