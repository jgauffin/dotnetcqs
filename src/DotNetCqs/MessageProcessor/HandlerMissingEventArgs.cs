using System;
using System.Security.Claims;
using DotNetCqs.DependencyInjection;

namespace DotNetCqs.MessageProcessor
{
    /// <summary>
    ///     There is no handler registered for the inbound message.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         You can either decide to add a handler manually by using <see cref="SetHandler" />, ignore the message (do
    ///         nothing)
    ///         or tell the library to throw an exception (<see cref="ThrowException" />).
    ///     </para>
    /// </remarks>
    public class HandlerMissingEventArgs : EventArgs
    {
        public HandlerMissingEventArgs(ClaimsPrincipal user, Message message, IHandlerScope scope)
        {
            User = user;
            Message = message;
            Scope = scope;
        }

        /// <summary>
        ///     State that was specified in <see cref="InvokingHandlerEventArgs" />
        /// </summary>
        public object ApplicationState { get; set; }

        public Message Message { get; }
        public IHandlerScope Scope { get; }

        /// <summary>
        ///     Throw an exception (to flow this is an error in the processing pipeline).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If not set, the inbound message will just be ignored.
        ///     </para>
        /// </remarks>
        public bool ThrowException { get; set; }

        public ClaimsPrincipal User { get; }

        /// <summary>
        /// </summary>
        internal object Handler { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="handler">Should implement <see cref="IMessageHandler{TMessage}" />.</param>
        public void SetHandler(object handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            Handler = handler;
        }
    }
}