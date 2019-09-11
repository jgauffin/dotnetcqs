using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Queues;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using AzureMessage= Microsoft.Azure.ServiceBus.Message;

namespace DotNetCqs.Azure.ServiceBus
{
    public class AzureMessageQueueSession : IMessageQueueSession
    {
        private readonly MessageConverter _converter;
        private readonly List<AzureMessage> _messagesReceived = new List<AzureMessage>();
        private readonly List<AzureMessage> _messagesToSend = new List<AzureMessage>();
        private readonly MessageReceiver _messageReceiver;
        private readonly MessageSender _messageSender;

        public AzureMessageQueueSession(MessageReceiver messageReceiver, MessageSender messageSender, IMessageSerializer<string> messageSerializer)
        {
            _messageReceiver = messageReceiver;
            _messageSender = messageSender;
            _converter = new MessageConverter(messageSerializer);
        }

        public async Task<Message> Dequeue(TimeSpan suggestedWaitPeriod)
        {
            var message = await _messageReceiver.ReceiveAsync(suggestedWaitPeriod);
            if (message == null)
                return null;

            _messagesReceived.Add(message);
            var tuple = _converter.FromAzureMessage(message);
            return tuple.Item2;
        }

        public async Task<DequeuedMessage> DequeueWithCredentials(TimeSpan suggestedWaitPeriod)
        {
            var dto = await _messageReceiver.ReceiveAsync(suggestedWaitPeriod);
            if (dto == null)
                return null;

            _messagesReceived.Add(dto);
            var tuple = _converter.FromAzureMessage(dto);
            return new DequeuedMessage(tuple.Item1, tuple.Item2);
        }

        public Task EnqueueAsync(ClaimsPrincipal principal, IReadOnlyCollection<Message> messages)
        {
            foreach (var message in messages)
            {
                var msg = _converter.ToAzureMessage(message, principal);
                _messagesToSend.Add(msg);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(IReadOnlyCollection<Message> messages)
        {
            foreach (var message in messages)
            {
                var msg = _converter.ToAzureMessage(message, null);
                _messagesToSend.Add(msg);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(ClaimsPrincipal principal, Message message)
        {
            var msg = _converter.ToAzureMessage(message, principal);
            _messagesToSend.Add(msg);
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(Message message)
        {
            var msg = _converter.ToAzureMessage(message, null);
            msg.TimeToLive = TimeSpan.FromMinutes(1);
            _messagesToSend.Add(msg);
            return Task.CompletedTask;
        }

        public async Task SaveChanges()
        {
            await _messageSender.SendAsync(_messagesToSend);
            _messagesToSend.Clear();

            foreach (var message in _messagesReceived)
            {
                await _messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
            }
            _messagesReceived.Clear();
        }

        public void Dispose()
        {
            _messagesToSend.Clear();

            foreach (var message in _messagesReceived)
                _messageReceiver.AbandonAsync(message.SystemProperties.LockToken).GetAwaiter().GetResult();
            _messagesReceived.Clear();
        }
    }
}