using System;
using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Messages;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Handlers
{
    class ActivateUserHandler : IMessageHandler<ActivateUser>
    {
        public async Task HandleAsync(IMessageContext context, ActivateUser message)
        {
            var user = await context.QueryAsync(new FindUser(){Id = 1});
            if (user != null)
                throw new InvalidOperationException("should not find a user");

            user = await context.QueryAsync(new FindUser() { Id = 2 });
            if (user == null)
                throw new InvalidOperationException("should find a user");

            await context.SendAsync(new UserActivated());
        }
    }
}
