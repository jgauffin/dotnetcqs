namespace DotNetCqs.Queues.Azure.ServiceBus
{
    public interface IExceptionLogger
    {
        void OnReceiveException(ReceiveExceptionContext context);
    }
}