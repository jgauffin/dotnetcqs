using System.Threading.Tasks;

namespace DotNetCqs.Tests.MessageProcessor
{
    public class MyHandler : IQueryHandler<MyQuery, string>
    {

        public string Result { get; set; } = null;
        public Task<string> HandleAsync(IMessageContext context, MyQuery query)
        {
            return Task.FromResult(Result);
        }
    }
}