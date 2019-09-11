//using System;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using DotNetCqs.DependencyInjection;
//using DotNetCqs.Tests.Helpers;
//using DotNetCqs.Tests.TestEntities;
//using FluentAssertions;
//using Xunit;

//namespace DotNetCqs.Tests.DependencyInjection
//{
//    public class DependencyInjectionMessageBusTests
//    {
//        [Fact]
//        public async Task Should_be_able_to_invoke_queries()
//        {
//            var expected = new OneResult() { Name = "King" };
//            var handler = new OneQueryHandler(expected);
//            var bus = TestHelpers.CreateBus(config => { config.RegisterHandler(handler); });

//            var actual = await bus.QueryAsync(ClaimsPrincipal.Current, new OneQuery());

//            actual.Should().BeSameAs(expected);
//        }

//        [Fact]
//        public void Should_require_only_one_query_handler()
//        {
//            var expected = new OneResult() { Name = "King" };
//            var handler = new OneQueryHandler(expected);
//            var isQuery = new OneQuery().IsQuery();
//            var anotherHandler = new OneQueryHandler(null);
//            var bus = TestHelpers.CreateBus(config =>
//            {
//                config.RegisterHandler(handler);
//                config.RegisterHandler(anotherHandler);
//            });

//            Func<Task> actual = () => bus.QueryAsync(ClaimsPrincipal.Current, new OneQuery());

//            actual.Should().Throw<OnlyOneQueryHandlerAllowedException>();
//        }


//        [Fact]
//        public async Task Should_be_able_to_invoke_a_simple_handler()
//        {
//            var handler = new TestHandler<Simple>();
//            var bus = TestHelpers.CreateBus(config => { config.RegisterHandler(handler); });

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            handler.Invoked.Should().BeTrue();
//        }

//        [Fact]
//        public async Task should_be_able_to_publish_a_message_from_a_handler()
//        {
//            var invoked = false;
//            var handler = new TestHandler<string>((ctx, msg) => invoked = true);
//            var bus = TestHelpers.CreateBus(config =>
//            {
//                config.RegisterConcrete<PublishingHandler>();
//                config.RegisterHandler(handler);
//            });

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            invoked.Should().BeTrue();
//        }

//        [Fact]
//        public async Task should_not_publish_second_message_if_first_one_fails()
//        {
//            var invoked = false;
//            var handler2 = new TestHandler<string>((ctx, msg) => invoked = true);
//            var handler1 = new TestHandler<Simple>((ctx, msg) =>
//            {
//                ctx.SendAsync("Hello world");
//                throw new InvalidOperationException();
//            });
//            var bus = TestHelpers.CreateBus(x =>
//            {
//                x.RegisterHandler(handler1);
//                x.RegisterHandler(handler2);
//            });

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            invoked.Should().BeFalse();
//        }

//        [Fact]
//        public async Task should_report_exception_in_ScopeClosing()
//        {
//            var gotException = false;
//            var handler1 = new TestHandler<Simple>((ctx, msg) =>
//            {
//                ctx.SendAsync("Hello world");
//                throw new InvalidOperationException();
//            });
//            var bus = TestHelpers.CreateBus(x => { x.RegisterHandler(handler1); });

//            bus.ScopeClosing += (sender, args) => gotException = args.Exception != null;

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            gotException.Should().BeTrue();
//        }

//        [Fact]
//        public async Task should_report_exception_in_HandlerInvoked()
//        {
//            var gotException = false;
//            var handler1 = new TestHandler<Simple>((ctx, msg) => throw new InvalidOperationException());
//            var bus = TestHelpers.CreateBus(x => { x.RegisterHandler(handler1); });

//            bus.HandlerInvoked += (sender, args) => gotException = args.Exception != null;

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            gotException.Should().BeTrue();
//        }

//        [Fact]
//        public async Task should_measure_time_for_handler_invocation_when_subscribing_on_handlerClosed_event()
//        {
//            var time = TimeSpan.Zero;
//            var handler1 = new TestHandler<Simple>((ctx, msg) => Task.Delay(50));
//            var bus = TestHelpers.CreateBus(x => { x.RegisterHandler(handler1); });

//            bus.HandlerInvoked += (sender, args) => time = args.ExecutionTime;

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            time.Should().BeCloseTo(TimeSpan.FromMilliseconds(50));
//        }

//        [Fact]
//        public async Task should_measure_time_for_handler_invocation_when_subscribing_on_handlerClosed_event_even_if_an_Exception_is_thrown_to_aid_diagnostics()
//        {
//            HandlerInvokedEventArgs e = null;
//            var handler1 = new TestHandler<Simple>(async (ctx, msg) =>
//            {
//                await Task.Delay(50);
//                throw new InvalidOperationException();
//            });
//            var bus = TestHelpers.CreateBus(x => { x.RegisterHandler(handler1); });

//            bus.HandlerInvoked += (sender, args) => e = args;

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            e.ExecutionTime.Should().BeCloseTo(TimeSpan.FromMilliseconds(50), 30);
//            e.Exception.Should().BeOfType<InvalidOperationException>();
//        }

//        [Fact]
//        public async Task Should_allow_multiple_handlers_for_the_same_message()
//        {
//            var invocations = "";
//            HandlerInvokedEventArgs e = null;
//            var handler1 = new TestHandler<Simple>((ctx, msg) =>
//            {
//                invocations += "A";
//            });
//            var handler2 = new SimpleHandler(() =>
//            {
//                invocations += "B";
//            });
//            var bus = TestHelpers.CreateBus(x =>
//            {
//                x.RegisterHandler(handler1);
//                x.RegisterHandler(handler2);
//            });

//            bus.HandlerInvoked += (sender, args) => e = args;

//            await bus.SendAsync(ClaimsPrincipal.Current, new Simple());

//            invocations.Should().Contain("A");
//            invocations.Should().Contain("B");
//        }

//    }
//}