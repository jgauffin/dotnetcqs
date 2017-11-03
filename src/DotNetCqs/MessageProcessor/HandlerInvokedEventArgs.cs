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

        /// <summary>
        /// Handler that was invoked.
        /// </summary>
        public object Handler { get; }

        /// <summary>
        ///     Pretend like the exception didn't happen.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Typically used when you have multiple handlers or a message and want the others to be able to complete.
        ///     </para>
        ///     <para>
        ///         When setting this field you have probably logged the exception or taken any other measures to be able
        ///         to correct the failing handler later.
        ///     </para>
        /// </remarks>
        public bool IgnoreException { get; set; }

        public Message Message { get; }

        public IHandlerScope Scope { get; }
    }
}