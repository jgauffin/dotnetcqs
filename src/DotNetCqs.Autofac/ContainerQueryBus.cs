using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Autofac;

namespace DotNetCqs.Autofac
{
    public class ContainerQueryBus : IQueryBus
    {
        private readonly IContainer _container;

        public ContainerQueryBus(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam><param name="query">Query to execute.</param>
        /// <returns>
        /// Task which will complete once we've got the result (or something failed, like a query wait timeout).
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">query</exception>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            var handler = typeof (IQueryHandler<,>).MakeGenericType(query.GetType(), typeof (TResult));
            var collectionType = typeof(IEnumerable<>).MakeGenericType(handler);

            using (var scope = _container.BeginLifetimeScope())
            {
                var result = ((IEnumerable<object>)scope.Resolve(collectionType));
                var handlers = result.ToArray();

                if (handlers.Length == 0)
                    throw new CqsHandlerMissingException(query.GetType());
                if (handlers.Length != 1)
                    throw new OnlyOneHandlerAllowedException(query.GetType());

                var method = handler.GetMethod("ExecuteAsync");
                try
                {
                    var task = (Task) method.Invoke(handlers[0], new object[] {query});
                    await task;
                    return ((dynamic) task).Result;
                }
                catch (TargetInvocationException exception)
                {
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                    throw new Exception("this will never happen");
                }
                
            }
        }
    }

}