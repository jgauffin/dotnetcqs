using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Queues;

namespace DotNetCqs.DependencyInjection
{
    public class MessageInvoker : IMessageInvoker
    {
        private readonly IHandlerScope _scope;
        private IOutboundMessageRouter _outboundMessageRouter;
        public LoggerHandler Logger;

        public MessageInvoker(IHandlerScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public MessageInvoker(IHandlerScope scope, IOutboundMessageRouter outboundMessageRouter)
        {
            _outboundMessageRouter = outboundMessageRouter;
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        
        /// <summary>
        ///     Create a task per handler and invoke them in parallel.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Activate this option if you are sure of that all handlers work with different (database) resources, or you'll
        ///         sooner or later get deadlocks.
        ///     </para>
        /// </remarks>
        public bool InvokeHandlersInParallel { get; set; }

        public async Task ProcessAsync(IInvocationContext context, Message message)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (message == null) throw new ArgumentNullException(nameof(message));

            var messageContext = new MessageInvocationContext(context.Principal, message, this);

            // when messages arrive through the listener there is not difference between 
            // messages and queries, since the result should be delivered over the queue.
            if (IsQuery(message.Body))
                await InvokeQueryHandler(messageContext, message);
            else
                await InvokeMessageHandlers(messageContext, message);


            // someone else is taking care of the outbound messages
            if (_outboundMessageRouter != null)
            {
                Logger?.Invoke(LogLevel.Debug, "", "Invoking IOutboundMessageRouter.");
                await _outboundMessageRouter.SendAsync(messageContext);
            }
            foreach (var msg in messageContext.OutboundMessages)
            {
                Logger?.Invoke(LogLevel.Info, message.MessageId.ToString("N"), $"Sending {msg.Body.GetType()}");
                await context.SendAsync(msg);
            }

            foreach (var msg in messageContext.Replies)
            {
                Logger?.Invoke(LogLevel.Info, message.MessageId.ToString("N"), $"Replying with {msg.Body.GetType()}");
                await context.ReplyAsync(msg);
            }
                
        }

        public Task ProcessAsync(IInvocationContext context, object message)
        {
            return ProcessAsync(context, new Message(message));
        }

        /// <summary>
        ///     Triggered when a message handler have been invoked.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The <c>Exception</c> property can contains error(s) from the invocation
        ///     </para>
        /// </remarks>
        public event EventHandler<HandlerInvokedEventArgs> HandlerInvoked;

        /// <summary>
        ///     The inbound message do not have a registered handler.
        /// </summary>
        public event EventHandler<HandlerMissingEventArgs> HandlerMissing;

        /// <summary>
        ///     Triggered when a handler is about to be invoked.
        /// </summary>
        public event EventHandler<InvokingHandlerEventArgs> InvokingHandler;

        private static Type GetQueryResultType(object cqsObject)
        {
            if (cqsObject == null) throw new ArgumentNullException(nameof(cqsObject));
            var baseType = cqsObject.GetType().BaseType;
            while (baseType != null)
            {
                if (baseType.FullName.StartsWith("DotNetCqs.Query"))
                    return baseType.GetGenericArguments()[0];

                baseType = baseType.BaseType;
            }
            throw new InvalidOperationException("Failed to find result type for " + cqsObject.GetType());
        }

        private async Task InvokeMessageHandlers(IMessageContext context, Message message)
        {
            var type = typeof(IMessageHandler<>).MakeGenericType(message.Body.GetType());
            var handlers = _scope.Create(type).ToList();
            if (handlers.Count == 0)
            {
                Logger?.Invoke(LogLevel.Warning, message.MessageId.ToString("N"), "Missing handler for " + message.Body.GetType());
                var e = new HandlerMissingEventArgs(context.Principal, message, _scope);
                HandlerMissing?.Invoke(this, e);
                if (e.Handler != null)
                    handlers.Add(e.Handler);
                else if (e.ThrowException)
                    throw new NoHandlerRegisteredException("Failed to find handler for '" + message.Body + "'.");
                else
                    return;
            }


            if (InvokeHandlersInParallel)
            {
                var tasks = handlers.Select(async handler =>
                {
                    var mi = handler.GetType().GetMethod("HandleAsync");
                    var task = (Task) mi.Invoke(handler, new[] {context, message.Body});
                    Stopwatch sw = null;
                    var args1 = new InvokingHandlerEventArgs(_scope, handler, message);
                    try
                    {
                        if (HandlerInvoked != null)
                            sw = Stopwatch.StartNew();

                    Logger?.Invoke(LogLevel.Info, message.MessageId.ToString("N"), "Invoking " + handler.GetType());
                        InvokingHandler?.Invoke(this, args1);
                        await task;
                        sw?.Stop();
                        HandlerInvoked?.Invoke(this,
                            new HandlerInvokedEventArgs(_scope, handler, message, args1, sw?.Elapsed ?? TimeSpan.Zero));
                    }
                    catch (Exception ex)
                    {
                    Logger?.Invoke(LogLevel.Error, message.MessageId.ToString("N"),
                        $"Handler failed: {handler.GetType()}, Exception: {ex}");
                        var e = new HandlerInvokedEventArgs(_scope, handler, message, args1.ApplicationState,
                            sw?.Elapsed ?? TimeSpan.Zero)
                        {
                            Exception = ex
                        };
                        HandlerInvoked?.Invoke(this, e);

                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                }).ToArray();

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception)
                {
                    var failedHandlers = new List<FailedHandler>();
                    for (var i = 0; i < handlers.Count; i++)
                    {
                        var task = tasks[i];
                        var handler = handlers[i];
                        if (task.IsFaulted)
                            failedHandlers.Add(new FailedHandler
                            {
                                Exception = task.Exception.InnerException,
                                HandlerType = handler.GetType()
                            });
                    }
                    throw new HandlersFailedException(message, failedHandlers);
                }

            }
            else
            {
                var failedHandlers = new List<FailedHandler>();
                foreach (var handler in handlers)
                {
                    var mi = handler.GetType().GetMethod("HandleAsync");
                    var task = (Task)mi.Invoke(handler, new[] { context, message.Body });
                    Stopwatch sw = null;
                    var args1 = new InvokingHandlerEventArgs(_scope, handler, message);
                    try
                    {
                        if (HandlerInvoked != null)
                            sw = Stopwatch.StartNew();

                        InvokingHandler?.Invoke(this, args1);
                        await task;
                        sw?.Stop();
                        HandlerInvoked?.Invoke(this,
                            new HandlerInvokedEventArgs(_scope, handler, message, args1, sw?.Elapsed ?? TimeSpan.Zero));
                    }
                    catch (Exception ex)
                    {
                        var e = new HandlerInvokedEventArgs(_scope, handler, message, args1.ApplicationState,
                            sw?.Elapsed ?? TimeSpan.Zero)
                        {
                            Exception = ex
                        };
                        HandlerInvoked?.Invoke(this, e);
                        failedHandlers.Add(new FailedHandler(){Exception = ex, HandlerType = handlers.GetType()});
                    }

                }

                if (failedHandlers.Any())
                    throw new HandlersFailedException(message, failedHandlers);
            }
        }

        private async Task InvokeQueryHandler(MessageInvocationContext context, Message message)
        {
            var resultType = GetQueryResultType(message.Body);

            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(message.Body.GetType(), resultType);
            var handlers = _scope.Create(handlerType).ToList();
            if (handlers.Count == 0)
                throw new NoHandlerRegisteredException($"Missing handler for query '{message.Body}'.");
            if (handlers.Count > 1)
                throw new OnlyOneQueryHandlerAllowedException(
                    $"Queries may only have one handler, '{message.Body}' got [{string.Join(", ", handlers.Select(x => x.GetType().FullName))}].");
            var handler = handlers[0];

            Task task;
            Stopwatch sw = null;
            var args1 = new InvokingHandlerEventArgs(_scope, handler, message);
            try
            {
                if (HandlerInvoked != null)
                    sw = Stopwatch.StartNew();

                InvokingHandler?.Invoke(this, args1);

                task = (Task) handler
                    .GetType()
                    .GetMethod("HandleAsync")
                    .Invoke(handler, new[] {context, message.Body});
                await task;

                sw?.Stop();
                HandlerInvoked?.Invoke(this,
                    new HandlerInvokedEventArgs(_scope, handler, message, args1, sw?.Elapsed ?? TimeSpan.Zero));
            }
            catch (Exception ex)
            {
                var e = new HandlerInvokedEventArgs(_scope, handler, message, args1.ApplicationState,
                    sw?.Elapsed ?? TimeSpan.Zero)
                {
                    Exception = ex
                };
                HandlerInvoked?.Invoke(this, e);

                ExceptionDispatchInfo.Capture(ex).Throw();
                return;
            }

            var resultProperty = typeof(Task<>).MakeGenericType(resultType).GetProperty("Result");
            var result = resultProperty.GetValue(task);
            await context.ReplyAsync(result);
        }

        private static bool IsQuery(object cqsObject)
        {
            if (cqsObject == null) throw new ArgumentNullException(nameof(cqsObject));
            var baseType = cqsObject.GetType().BaseType;
            while (baseType != null)
            {
                if (baseType.FullName.StartsWith("DotNetCqs.Query"))
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}