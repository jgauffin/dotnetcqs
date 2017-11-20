namespace DotNetCqs.Queues.AdoNet.IntegrationTests.CompleteTest.Messages.Messages
{
    public class FindUser : Query<FindUserResult>
    {
        public int Id { get; set; }
    }

    public class FindUserResult
    {
    }
}
