using System;

namespace DotNetCqs
{
    /// <summary>
    /// Commands are instructions that change application state. Do not that it's just the instruction and not the actual state changer (which is the class that implements <see cref="ICommandHandler{TCommand}"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Our recommendation is that a command encapsulate an entire use case. A typical command can be <c>PostForumMessage</c> or <c>ApplySeasonalDiscount</c>. But doing so it's
    /// much easier to scale the application by moving the command processing to a different server (or servers).
    /// </para>
    /// <para>
    /// Commands do not have a response. Hence you need to assign any id to the command before executing it. If you *really* require a return value, use <see cref="Request{T}"/> instead. Commands
    /// uses less system resources though.
    /// </para>
    /// </remarks>
    public class Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        public Command()
        {
            CommandId = GuidFactory.Create();
        }

        /// <summary>
        /// Id identifying this command (as it might be used in inter process communication).
        /// </summary>
        public Guid CommandId { get; private set; }
    }
}