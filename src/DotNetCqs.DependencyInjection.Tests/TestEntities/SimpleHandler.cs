using System;
using System.Threading.Tasks;

namespace DotNetCqs.Tests.TestEntities
{
    public class SimpleHandler : TestHandler<Simple>
    {
        private readonly Action _x;

        public SimpleHandler(Action x)
        {
            _x = x;
        }

        protected override Task OnHandleAsync(IMessageContext context, Simple message)
        {
            _x();
            return base.OnHandleAsync(context, message);
        }
    }
}