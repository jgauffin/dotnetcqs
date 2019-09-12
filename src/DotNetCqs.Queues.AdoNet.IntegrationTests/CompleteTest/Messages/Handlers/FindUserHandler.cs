using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Messages;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Handlers
{
    public class FindUserHandler : IQueryHandler<FindUser, FindUserResult>
    {
        public Task<FindUserResult> HandleAsync(IMessageContext context, FindUser query)
        {
            return query.Id == 1
                ? Task.FromResult<FindUserResult>(null)
                : Task.FromResult(new FindUserResult());
        }
    }
}