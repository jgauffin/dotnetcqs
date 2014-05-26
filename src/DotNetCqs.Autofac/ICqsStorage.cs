using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Autofac
{
    /// <summary>
    ///     Storage for messages to process
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Used to be able to store messages until they are being processed, so that we doesn't loose any if the
    ///         application goes down.
    ///     </para>
    /// </remarks>
    public interface ICqsStorage
    {
        Encoding Encoding { get; set; }
        Task<Command> PopCommandAsync();
        Task<ApplicationEvent> PopEventAsync();
        Task<IQuery> PopQueryAsync();
        Task<IRequest> PopRequestAsync();
        Task PushAsync(Command command);
        Task PushAsync<T>(Request<T> request);
        Task PushAsync(ApplicationEvent appEvent);
        Task PushAsync<T>(Query<T> query);
    }
}