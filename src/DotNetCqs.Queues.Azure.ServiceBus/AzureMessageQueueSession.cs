using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs.Logging;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using AzureMessage= Microsoft.Azure.ServiceBus.Message;

namespace DotNetCqs.Queues.Azure.ServiceBus
{
    public class AzureMessageQueueSession : IMessageQueueSession
    {
        private readonly MessageConverter _converter;
        private readonly List<AzureMessage> _messagesReceived = new List<AzureMessage>();
        private readonly List<AzureMessage> _messagesToSend = new List<AzureMessage>();
        private readonly string _queueName;
        private readonly MessageReceiver _messageReceiver;
        private readonly MessageSender _messageSender;
        private ILogger _logger = LogConfiguration.LogFactory.CreateLogger(typeof(AzureMessageQueueSession));

        public AzureMessageQueueSession(string queueName, MessageReceiver messageReceiver, MessageSender messageSender, IMessageSerializer<string> messageSerializer)
        {
            _queueName = queueName;
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
                _logger.Debug(_queueName, "Enqueueing", principal, message);
                var msg = _converter.ToAzureMessage(message, principal);
                _messagesToSend.Add(msg);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(IReadOnlyCollection<Message> messages)
        {
            foreach (var message in messages)
            {
                _logger.Debug(_queueName, "Enqueueing", messageBeingProcessed: message);
                var msg = _converter.ToAzureMessage(message, null);
                _messagesToSend.Add(msg);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(ClaimsPrincipal principal, Message message)
        {
            _logger.Debug(_queueName, "Enqueueing", principal, message);
            var msg = _converter.ToAzureMessage(message, principal);
            _messagesToSend.Add(msg);
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(Message message)
        {
            _logger.Debug(_queueName, "Enqueueing", messageBeingProcessed: message);
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