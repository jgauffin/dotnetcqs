using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCqs.Queues;
using Microsoft.ServiceBus.Messaging;

namespace DotNetCqs.Azure.ServiceBus
{
    public class AzureMessageQueue : IMessageQueue
    {
        private readonly QueueClient _queueClient;

        public AzureMessageQueue(string queueName, QueueClient queueClient)
        {
            Name = queueName;
            _queueClient = queueClient;
        }

        public string Name { get; }

        public IMessageSerializer<string> MessageSerializer { get; set; }

        public IMessageQueueSession BeginSession()
        {
            return new AzureMessageQueueSession(_queueClient, MessageSerializer);
        }

        public async Task ReleaseEnqueueAsync(object identifier)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            var msgs = (List<BrokeredMessage>) identifier;
            await _queueClient.SendBatchAsync(msgs);
        }
    }
}