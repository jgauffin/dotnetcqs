using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Tests.TestEntities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNetCqs.Tests.DependencyInjection
{
    public class MessageInvokerTests
    {
        [Fact]
        public async Task Should_invoke_message_handler()
        {
            var scope = Substitute.For<IHandlerScope>();
            var handler = new TestHandler<Simple>();
            scope.Create(typeof(IMessageHandler<Simple>)).Returns(new[] {handler});
            var msg = new Message(new Simple());
            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");

            await sut.ProcessAsync(context, msg);

            handler.Invoked.Should().BeTrue();
        }

        [Fact]
        public async Task Should_invoke_all_message_handler_for_the_given_message()
        {
            var scope = Substitute.For<IHandlerScope>();
            var handler = new TestHandler<Simple>();
            var handler2 = new TestHandler<Simple>();
            scope.Create(typeof(IMessageHandler<Simple>)).Returns(new[] { handler, handler2 });
            var msg = new Message(new Simple());
            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");

            await sut.ProcessAsync(context, msg);

            handler.Invoked.Should().BeTrue();
            handler2.Invoked.Should().BeTrue();
        }

        [Fact]
        public async Task Should_let_exceptions_flow_to_allow_caller_to_decide_appropiate_response()
        {
            var scope = Substitute.For<IHandlerScope>();
            var handler = new TestHandler<Simple>();
            var handler2 = new TestHandler<Simple>((ctx,msg2) => throw new DataException("Here comes Lore!"));
            scope.Create(typeof(IMessageHandler<Simple>)).Returns(new[] { handler, handler2 });
            var msg = new Message(new Simple());
            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");

            Func<Task> actual = () => sut.ProcessAsync(context, msg);

            (await actual.Should().ThrowAsync<HandlersFailedException>())
                .And.FailedHandlers[0].Exception.Message.Should().Be("Here comes Lore!");
        }

        [Fact]
        public async Task Should_report_all_failed_handlers_in_the_exception()
        {
            var scope = Substitute.For<IHandlerScope>();
            var handler = new TestHandler<Simple>((ctx, msg2) => throw new InvalidOperationException("No Data :("));
            var handler2 = new TestHandler<Simple>((ctx, msg2) => throw new DataException("Here comes Lore!"));
            scope.Create(typeof(IMessageHandler<Simple>)).Returns(new[] { handler, handler2 });
            var msg = new Message(new Simple());
            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");

            Func<Task> actual = () => sut.ProcessAsync(context, msg);

            (await actual.Should().ThrowAsync<HandlersFailedException>())
                .And.FailedHandlers.Count.Should().Be(2);
        }

        [Fact]
        public async Task Should_be_able_to_execute_query_sucessfully()
        {
            var scope = Substitute.For<IHandlerScope>();
            var result = new OneResult();
            var handler = new OneQueryHandler(result);
            scope.Create(typeof(IQueryHandler<OneQuery, OneResult>)).Returns(new[] { handler});

            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");
            await sut.ProcessAsync(context, new OneQuery());

            context.Replies.First().Body.Should().BeSameAs(result);
        }

        [Fact]
        public async Task Should_abort_if_multiple_query_handlers_have_been_registered()
        {
            var scope = Substitute.For<IHandlerScope>();
            var result = new OneResult();
            var handler = new OneQueryHandler(result);
            scope.Create(typeof(IQueryHandler<OneQuery, OneResult>)).Returns(new[] { handler, handler });

            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");
            Func<Task> actual = () => sut.ProcessAsync(context, new OneQuery());

            await actual.Should().ThrowAsync<OnlyOneQueryHandlerAllowedException>();
        }

        [Fact]
        public async Task Should_require_a_query_handler()
        {
            var scope = Substitute.For<IHandlerScope>();
            var result = new OneResult();
            scope.Create(typeof(IQueryHandler<OneQuery, OneResult>)).Returns(new object[0]);

            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");
            Func<Task> actual = () => sut.ProcessAsync(context, new OneQuery());

            await actual.Should().ThrowAsync<NoHandlerRegisteredException>();
        }

        [Fact]
        public async Task Should_invoke_second_level_queries_directly()
        {
            var scope = Substitute.For<IHandlerScope>();
            var result = new OneResult();
            QueryTwoResult expected = null;
            var handler = new OneQueryHandler(result, (messageContext, query) =>
            {
                expected=messageContext.QueryAsync(new QueryTwo()).GetAwaiter().GetResult();
            });
            var result2 = new QueryTwoResult();
            var handler2=new QueryTwoHandler(result2);
            scope.Create(typeof(IQueryHandler<OneQuery, OneResult>)).Returns(new[] { handler });
            scope.Create(typeof(IQueryHandler<QueryTwo, QueryTwoResult>)).Returns(new[] { handler2 });

            var sut = new MessageInvoker(scope);
            var context = new ExecuteQueriesInvocationContext(ClaimsPrincipal.Current, sut, "Direct");
            await sut.ProcessAsync(context, new OneQuery());

            expected.Should().BeSameAs(result2, "because it should not be enqueued as queries that comes from the queue");
            context.Replies.Count.Should().Be(1, "because only result from first queue should be enqueued");
            context.Replies.First().Body.Should().Be(result);
            context.Messages.Should().BeEmpty("because no messages were enqueued by the handlers");
        }
    }
}
