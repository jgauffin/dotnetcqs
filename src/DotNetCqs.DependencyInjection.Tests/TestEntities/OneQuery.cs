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

    public class OneAsyncQueryHandler : IQueryHandler<OneQuery, OneResult>
    {
        private readonly OneResult _result;
        private Func<IMessageContext, OneQuery, Task> _action;
        public OneAsyncQueryHandler(OneResult result)
        {
            _result = result;
        }
        public OneAsyncQueryHandler(OneResult result, Func<IMessageContext, OneQuery, Task> action)
        {
            _result = result;
            _action = action;
        }
        public async Task<OneResult> HandleAsync(IMessageContext context, OneQuery message)
        {
            if (_action != null)
            {
                await _action(context, message);
            }

            return _result;
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
