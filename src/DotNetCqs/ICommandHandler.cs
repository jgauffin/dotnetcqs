using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// Used to execute a command
    /// </summary>
    /// <typeparam name="TCommand">Type of command to execute.</typeparam>
    /// <remarks>
    /// <para>
    /// There may only be one command handler per command. Anything else will result in an exception.
    /// </para>
    /// <para>
    /// Command handlers are typically discovered by using a inversion of control container.
    /// </para>
    /// </remarks>
    public interface ICommandHandler<in TCommand> where TCommand : Command
    {
        /// <summary>
        /// Execute a command asynchronously.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <returns>Task which will be completed once the command has been executed.</returns>
        Task ExecuteAsync(TCommand command);
    }
}