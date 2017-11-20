using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Messages;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Handlers
{
    public class LogAdminUpgrades : IMessageHandler<UserBecameAdmin>
    {
        private ManualResetEvent evt;

        public LogAdminUpgrades(ManualResetEvent evt)
        {
            this.evt = evt;
        }

        public Task HandleAsync(IMessageContext context, UserBecameAdmin message)
        {
            Console.WriteLine("Ohh my, is aDMIN!" + message);
            evt.Set();
            return Task.CompletedTask;
        }
    }
}
