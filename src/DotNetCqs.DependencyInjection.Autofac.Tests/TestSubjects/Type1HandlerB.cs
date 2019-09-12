using System;
using System.Threading.Tasks;

namespace DotNetCqs.DependencyInjection.Autofac.Tests.TestSubjects
{
    public class Type1HandlerB : IMessageHandler<Type1>
    {
        public Task HandleAsync(IMessageContext context, Type1 message)
        {
            throw new NotImplementedException();
        }
    }
}