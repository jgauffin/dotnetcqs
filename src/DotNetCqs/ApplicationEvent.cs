using System;

namespace DotNetCqs
{
    /// <summary>
    /// Application events are events which represents a change in the application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An application event might for instance be that a new forum post have been made. i.e. events that other parts of the applications can act upon. Application events should not be executed
    /// within the same transaction as the command/request that generated the application event. They should instead be queued up and executed within a small time frame.
    /// </para>
    /// <para>
    /// Handlers of application events may modify the application state. For instance by updating a read model (if you follow the CQRS pattern).
    /// </para>
    /// </remarks>
    public class ApplicationEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEvent"/> class.
        /// </summary>
        public ApplicationEvent()
        {
            EventId = GuidFactory.Create();
        }

        /// <summary>
        /// Id identifying this event instance  (as it might be used in inter process communication).
        /// </summary>
        public Guid EventId { get; set; }
    }
}
