using System;
using Microsoft.Azure.ServiceBus;

namespace DotNetCqs.Azure.ServiceBus
{
    public class ReceiveExceptionContext
    {
        public ReceiveExceptionContext(QueueClient queueClient, Exception exception)
        {
            QueueClient = queueClient;
            Exception = exception;
        }

        public ReceiveExceptionContext(QueueClient queueClient, Microsoft.Azure.ServiceBus.Message message,
            Exception exception)
        {
            QueueClient = queueClient;
            Message = message;
            Exception = exception;
        }

        public IQueueClient QueueClient { get; set; }
        public Microsoft.Azure.ServiceBus.Message Message { get; set; }
        public Exception Exception { get; set; }
        public bool IsLastFailure { get; set; }
    }
}