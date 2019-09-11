namespace DotNetCqs.Azure.ServiceBus
{
    public interface IExceptionLogger
    {
        void OnReceiveException(ReceiveExceptionContext context);
    }
}