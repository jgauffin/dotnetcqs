using System;
using System.Security.Claims;
using System.Security.Principal;

namespace DotNetCqs.DependencyInjection
{
    /// <summary>
    ///     Arguments for <see cref="DependencyInjectionMessageBus.ScopeClosing" />.
    /// </summary>
    public class ScopeClosingEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ScopeClosingEventArgs" />.
        /// </summary>
        /// <param name="scope">Created message scope</param>
        /// <param name="message">Message that triggered the scope creation</param>
        /// <param name="principal"></param>
        /// <param name="applicationState">
        ///     Application state (used by the library used to be able to pass information to the other
        ///     events for the same message)
        /// </param>
        public ScopeClosingEventArgs(IHandlerScope scope, Message message, object applicationState)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            ApplicationState = applicationState;
        }


        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        ///     State retained between ScopeCreated and ScopeClosing events
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This state will be supplied in <see cref="DependencyInjectionMessageBus.ScopeClosing" />,
        ///         <see cref="DependencyInjectionMessageBus.InvokingHandler" /> and
        ///         <see cref="DependencyInjectionMessageBus.HandlerInvoked" />.
        ///     </para>
        /// </remarks>
        public object ApplicationState { get; }

        /// <summary>
        ///     Exception if the handler failed.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Amount of time
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        ///     Message that triggered the scope creation
        /// </summary>
        public Message Message { get; }

        /// <summary>
        ///     Created message scope
        /// </summary>
        public IHandlerScope Scope { get; }
    }
}