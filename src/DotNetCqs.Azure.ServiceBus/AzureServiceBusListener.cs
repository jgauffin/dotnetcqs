using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Queues;
using Microsoft.Azure.ServiceBus;

namespace DotNetCqs.Azure.ServiceBus
{
    public class AzureServiceBusListener
    {
        private readonly IMessageBus _bus;
        private readonly IMessageInvoker _invoker;
        private readonly QueueClient _queueClient;
        private readonly MessageConverter _converter;

        public AzureServiceBusListener(IMessageInvoker invoker, QueueClient queueClient,
            IMessageSerializer<string> serializer)
        {
            _invoker = invoker;
            _queueClient = queueClient;
            _converter = new MessageConverter(serializer);
        }

        public IExceptionLogger ExceptionLogger { get; set; }

        public void Start()
        {
            var ops = new MessageHandlerOptions(OnException) {AutoComplete = false};
            _queueClient.RegisterMessageHandler(OnMessage2, ops);
        }

        public async Task Stop()
        {
            await _queueClient.CloseAsync();
        }

        private Task OnException(ExceptionReceivedEventArgs arg)
        {
            ExceptionLogger?.OnReceiveException(new ReceiveExceptionContext(_queueClient, arg.Exception)
                {IsLastFailure = true});
            return Task.CompletedTask;
        }

        private async Task OnMessage2(Microsoft.Azure.ServiceBus.Message message, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                var tuple = _converter.FromAzureMessage(message);
                var outboundMessages = new List<Message>();
                var ctx = new InvocationContext(message.Label, tuple.Item1, _invoker, outboundMessages);
                await _invoker.ProcessAsync(ctx, tuple.Item2);


                var azureMsgs = new List<Microsoft.Azure.ServiceBus.Message>();
                foreach (var outboundMessage in outboundMessages)
                {
                    var azureMsg = _converter.ToAzureMessage(outboundMessage, tuple.Item1);
                    azureMsgs.Add(azureMsg);
                }

                if (azureMsgs.Any())
                    await _queueClient.SendAsync(azureMsgs);
            }
            catch (Exception ex)
            {
                await _queueClient.AbandonAsync(message.SystemProperties.LockToken);
                ExceptionLogger?.OnReceiveException(new ReceiveExceptionContext(_queueClient, message, ex));
            }
        }

    }
}