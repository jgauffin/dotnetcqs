using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Messages;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Handlers
{
    class UpgradeToAdminHandler : IMessageHandler<UserActivated>
    {
        public Task HandleAsync(IMessageContext context, UserActivated message)
        {
            context.SendAsync(new UserBecameAdmin());
            return Task.CompletedTask;
        }
    }
}
