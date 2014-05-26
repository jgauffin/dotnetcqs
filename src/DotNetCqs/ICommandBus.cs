using System.Threading.Tasks;

namespace DotNetCqs
{
    /// <summary>
    /// Used to deliver commands for execution
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Request that a command should be executed.
        /// </summary>
        /// <typeparam name="T">Type of command to execute.</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>Task which completes once the command has been delivered (and NOT when it has been executed).</returns>
        /// <exception cref="System.ArgumentNullException">command</exception>
        /// <remarks>
        /// <para>
        /// The actual execution of an command can be done anywhere at any time. Do not expect the command to be executed just because this method returns. That just means
        /// that the command have been successfully delivered (to a queue or another process etc) for execution.
        /// </para>
        /// </remarks>
        Task ExecuteAsync<T>(T command) where T : Command;
    }

   
}