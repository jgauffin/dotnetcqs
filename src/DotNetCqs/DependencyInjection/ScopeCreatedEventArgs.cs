using System;
using System.Security.Claims;
using DotNetCqs.Queues;

namespace DotNetCqs.DependencyInjection
{
    /// <summary>
    ///     A new scope have been created for a message
    /// </summary>
    public class ScopeCreatedEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ScopeCreatedEventArgs" />.
        /// </summary>
        /// <param name="scope">Created scope</param>
        /// <param name="principal">Principal attached to the message</param>
        /// <param name="message">Message that triggered the scope creation</param>
        public ScopeCreatedEventArgs(IHandlerScope scope, ClaimsPrincipal principal, Message message)
        {
            Principal = principal;
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        ///     State retained between ScopeCreated and ScopeClosing events
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This state will be supplied in <see cref="QueueListener.ScopeClosing" />.
        ///     </para>
        /// </remarks>
        public object ApplicationState { get; set; }

        /// <summary>
        ///     Messaging being processed
        /// </summary>
        public Message Message { get; }

        public ClaimsPrincipal Principal { get; }

        /// <summary>
        ///     Scope used to locate handlers.
        /// </summary>
        public IHandlerScope Scope { get; set; }
    }
}