using System;
using System.Collections.Generic;
using Griffin.Container;

namespace DotNetCqs.DependencyInjection.GriffinContainer
{
    public class GriffinScopeAdapter : IHandlerScope
    {
        private IChildContainer _scope;

        public GriffinScopeAdapter(IChildContainer scope)
        {
            _scope = scope;
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _scope = null;
        }

        public IEnumerable<object> ResolveAll(Type messageHandlerType)
        {
            return _scope.ResolveAll(messageHandlerType);
        }

        public object Resolve(Type concreteHandlerType)
        {
            return _scope.Resolve(concreteHandlerType);
        }

        public IEnumerable<object> Create(Type messageHandlerServiceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ResolveDependency<T>()
        {
            throw new NotImplementedException();
        }
    }
}