using System;
using System.Threading.Tasks;

namespace DotNetCqs.Autofac.Tests.TestSubjects
{
    public class Type2Handler : IQueryHandler<Type2, Type2Result>
    {
        public Task<Type2Result> HandleAsync(IMessageContext context, Type2 query)
        {
            throw new NotImplementedException();
        }
    }
}