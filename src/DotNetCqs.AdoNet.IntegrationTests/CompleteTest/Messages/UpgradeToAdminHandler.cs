using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages
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
