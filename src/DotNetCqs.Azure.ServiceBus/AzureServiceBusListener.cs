using System;
using System.Threading.Tasks;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Queues;
using Microsoft.ServiceBus.Messaging;

namespace DotNetCqs.Azure.ServiceBus
{
    public class AzureServiceBusListener
    {
        private readonly IMessageBus _bus;
        private readonly QueueClient _queueClient;
        private readonly BrokeredMessageConverter _converter;

        public AzureServiceBusListener(IMessageInvoker invoker, QueueClient queueClient,
            IMessageSerializer<string> serializer)
        {
            _queueClient = queueClient;
            _converter = new BrokeredMessageConverter(serializer);
        }

        public void Start()
        {
            _queueClient.OnMessageAsync(OnMessage, new OnMessageOptions {AutoComplete = false});
        }

        private async Task OnMessage(BrokeredMessage brokeredMessage)
        {
            try
            {
                var tuple = _converter.FromBrokeredMessage(brokeredMessage);
                //TODO: Complete.
            }
            catch (Exception ex)
            {
                //TODO: Log.
                await brokeredMessage.AbandonAsync();
            }
        }
    }
}