using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCqs.Tests.TestEntities
{
    public class OneQuery : Query<OneResult>
    {
    }

    public class OneResult
    {
        public string Name { get; set; }
    }

    public class OneQueryHandler : IQueryHandler<OneQuery, OneResult>
    {
        private readonly OneResult _result;
        private Action<IMessageContext, OneQuery> _action;
        public OneQueryHandler(OneResult result)
        {
            _result = result;
        }
        public OneQueryHandler(OneResult result, Action<IMessageContext, OneQuery> action)
        {
            _result = result;
            _action = action;
        }
        public Task<OneResult> HandleAsync(IMessageContext context, OneQuery message)
        {
            _action?.Invoke(context, message);
            return Task.FromResult(_result);
        }
    }


    public class QueryTwo : Query<QueryTwoResult>
    {
    }

    public class QueryTwoResult
    {
        public string Name { get; set; }
    }

    public class QueryTwoHandler : IQueryHandler<QueryTwo, QueryTwoResult>
    {
        private readonly QueryTwoResult _result;
        private Action<IMessageContext, QueryTwo> _action;
        public QueryTwoHandler(QueryTwoResult result)
        {
            _result = result;
        }
        public QueryTwoHandler(QueryTwoResult result, Action<IMessageContext, QueryTwo> action)
        {
            _result = result;
            _action = action;
        }
        public Task<QueryTwoResult> HandleAsync(IMessageContext context, QueryTwo message)
        {
            _action?.Invoke(context, message);
            return Task.FromResult(_result);
        }
    }
}
