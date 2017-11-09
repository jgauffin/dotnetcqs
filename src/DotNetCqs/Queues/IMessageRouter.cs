using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Queues
{
    /// <summary>
    /// Routes outbound messages
    /// </summary>
    public interface IMessageRouter
    {

        Task SendAsync( Message message);
        Task SendAsync(IReadOnlyCollection<Message> messages);

        Task SendAsync(ClaimsPrincipal principal, Message message);
        Task SendAsync(ClaimsPrincipal principal, IReadOnlyCollection<Message> messages);
    }
}
