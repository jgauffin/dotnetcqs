using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Queues;
using Microsoft.ServiceBus.Messaging;

namespace DotNetCqs.Azure.ServiceBus
{
    public class AzureMessageQueueSession : IMessageQueueSession
    {
        private readonly BrokeredMessageConverter _converter;
        private readonly List<BrokeredMessage> _messagesRecieved = new List<BrokeredMessage>();
        private readonly List<BrokeredMessage> _messagesToSend = new List<BrokeredMessage>();
        private readonly QueueClient _queueClient;

        public AzureMessageQueueSession(QueueClient queueClient, IMessageSerializer<string> messageSerializer)
        {
            _queueClient = queueClient;
            _converter = new BrokeredMessageConverter(messageSerializer);
        }

        public async Task<Message> Dequeue(TimeSpan suggestedWaitPeriod)
        {
            var brokeredMessage = await _queueClient.ReceiveAsync(suggestedWaitPeriod);
            _messagesRecieved.Add(brokeredMessage);
            var tuple = _converter.FromBrokeredMessage(brokeredMessage);
            return tuple.Item2;
        }

        public async Task<DequeuedMessage> DequeueWithCredentials(TimeSpan suggestedWaitPeriod)
        {
            var dto = await _queueClient.ReceiveAsync(suggestedWaitPeriod);
            _messagesRecieved.Add(dto);
            var tuple = _converter.FromBrokeredMessage(dto);
            return new DequeuedMessage(tuple.Item1, tuple.Item2);
        }

        public Task EnqueueAsync(ClaimsPrincipal principal, IReadOnlyCollection<Message> messages)
        {
            foreach (var message in messages)
            {
                var msg = _converter.ToBrokeredMessage(message, principal);
                _messagesToSend.Add(msg);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(IReadOnlyCollection<Message> messages)
        {
            foreach (var message in messages)
            {
                var msg = _converter.ToBrokeredMessage(message, null);
                _messagesToSend.Add(msg);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(ClaimsPrincipal principal, Message message)
        {
            var msg = _converter.ToBrokeredMessage(message, principal);
            _messagesToSend.Add(msg);
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(Message message)
        {
            var msg = _converter.ToBrokeredMessage(message, null);
            _messagesToSend.Add(msg);
            return Task.CompletedTask;
        }

        public async Task SaveChanges()
        {
            await _queueClient.SendBatchAsync(_messagesToSend);
            _messagesToSend.Clear();

            foreach (var message in _messagesRecieved)
                await message.CompleteAsync();
            _messagesRecieved.Clear();
        }

        public void Dispose()
        {
            _messagesToSend.Clear();

            foreach (var message in _messagesRecieved)
                message.Abandon();
            _messagesRecieved.Clear();
        }
    }
}