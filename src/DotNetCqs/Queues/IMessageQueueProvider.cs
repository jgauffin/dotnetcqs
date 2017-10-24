namespace DotNetCqs.Queues
{
    public  interface IMessageQueueProvider
    {
        IMessageQueue Open(string queueName);
    }


}
