using System;
using System.Threading.Tasks;

namespace DotNetCqs.Tests.TestEntities
{
    public class TestHandler<T> : IMessageHandler<T>
    {
        private readonly Action<IMessageContext, T> _callback;
        private readonly Func<IMessageContext, T, Task> _callback2;

        public TestHandler(Action<IMessageContext, T> callback)
        {
            _callback = callback;
        }
        public TestHandler(Func<IMessageContext, T,Task> callback)
        {
            _callback2 = callback;
        }

        public TestHandler()
        {

        }
        public async Task HandleAsync(IMessageContext context, T message)
        {
            Invoked = true;
            Message = message;
            _callback?.Invoke(context, message);

            if (_callback2 != null)
                await _callback2.Invoke(context, message);

            await OnHandleAsync(context, message);
        }

        protected virtual Task OnHandleAsync(IMessageContext context, T message)
        {
            return Task.CompletedTask;
        }

        public T Message { get; set; }

        public bool Invoked { get; set; }
    }
}