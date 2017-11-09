using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages
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
