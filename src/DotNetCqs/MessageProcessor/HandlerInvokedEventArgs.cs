using System;
using DotNetCqs.DependencyInjection;

namespace DotNetCqs.MessageProcessor
{
    public class HandlerInvokedEventArgs : EventArgs
    {
        public HandlerInvokedEventArgs(IHandlerScope scope, object handler, Message message, object applicationState,
            TimeSpan executionTime)
        {
            Scope = scope;
            Handler = handler;
            Message = message;
            ApplicationState = applicationState;
            ExecutionTime = executionTime;
        }

        /// <summary>
        ///     State retained between ScopeCreated and ScopeClosing events
        /// </summary>
        public object ApplicationState { get; }

        /// <summary>
        ///     Exception if the handler failed.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Amount of time it took to execute the message handler.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        public object Handler { get; }
        public Message Message { get; }

        public IHandlerScope Scope { get; }
    }
}