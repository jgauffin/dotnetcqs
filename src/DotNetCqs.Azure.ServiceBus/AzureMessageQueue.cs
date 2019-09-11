using System;
using DotNetCqs.Queues;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace DotNetCqs.Azure.ServiceBus
{
    public class AzureMessageQueue : IMessageQueue
    {
        private readonly MessageSender _sender;
        private readonly MessageReceiver _receiver;

        public AzureMessageQueue(string connectionString, string queueName)
        {
            _sender = new MessageSender(connectionString, queueName);
            _receiver = new MessageReceiver(connectionString, queueName, ReceiveMode.PeekLock, new RetryExponential(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3), 5));
            Name = queueName;
        }

        public string Name { get; }

        public IMessageSerializer<string> MessageSerializer { get; set; }

        public IMessageQueueSession BeginSession()
        {
            return new AzureMessageQueueSession(_receiver, _sender, MessageSerializer);
        }

        //public async Task ReleaseEnqueueAsync(object identifier)
        //{
        //    if (identifier == null) throw new ArgumentNullException(nameof(identifier));
        //    var msgs = (List<Message>) identifier;
        //    await _queueClient.SendAsync(msgs);
        //}
    }
}