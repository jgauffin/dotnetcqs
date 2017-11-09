using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages
{
    class ActivateUserHandler : IMessageHandler<ActivateUser>
    {
        public Task HandleAsync(IMessageContext context, ActivateUser message)
        {
            context.SendAsync(new UserActivated());
            return Task.CompletedTask;
        }
    }
}
