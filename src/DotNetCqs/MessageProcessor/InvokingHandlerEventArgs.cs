using System;
using DotNetCqs.DependencyInjection;

namespace DotNetCqs.MessageProcessor
{
    public class InvokingHandlerEventArgs : EventArgs
    {
        public InvokingHandlerEventArgs(IHandlerScope scope, object handler, Message message)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Handler = handler;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        ///     State that will be passed on to <see cref="HandlerInvokedEventArgs" />.
        /// </summary>
        /// <remarks>
        ///     Use it if you need to clean up something once the handler have been executed.
        /// </remarks>
        public object ApplicationState { get; set; }

        public object Handler { get; }

        /// <summary>
        ///     Messaging being processed
        /// </summary>
        public Message Message { get; }

        /// <summary>
        ///     Scope used to locate handlers.
        /// </summary>
        public IHandlerScope Scope { get; set; }
    }
}