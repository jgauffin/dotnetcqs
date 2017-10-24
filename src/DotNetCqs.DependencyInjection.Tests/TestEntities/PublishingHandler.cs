using System.Threading.Tasks;

namespace DotNetCqs.Tests.TestEntities
{
    public class PublishingHandler : TestHandler<Simple>
    {
        protected override async Task OnHandleAsync(IMessageContext context, Simple message)
        {
            await context.SendAsync("Hello world");
        }
    }
}