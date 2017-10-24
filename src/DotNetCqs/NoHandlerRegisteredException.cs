namespace DotNetCqs
{
    /// <summary>
    ///     No handler have been registered for the given message.
    /// </summary>
    public class NoHandlerRegisteredException : CqsException
    {
        public NoHandlerRegisteredException(string errorMessage) : base(errorMessage)
        {
        }
    }
}