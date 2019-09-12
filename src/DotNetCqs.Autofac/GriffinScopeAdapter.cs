using System;
using System.Collections.Generic;
using Autofac;
using DotNetCqs.DependencyInjection;

namespace DotNetCqs.Autofac
{
    public class AutofacScopeAdapter : IHandlerScope
    {
        private ILifetimeScope _scope;

        public AutofacScopeAdapter(ILifetimeScope scope)
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
            var t = typeof(IEnumerable<>).MakeGenericType(messageHandlerType);
            return (IEnumerable<object>)_scope.Resolve(t);
        }

        public object Resolve(Type concreteHandlerType)
        {
            return _scope.Resolve(concreteHandlerType);
        }

        public IEnumerable<object> Create(Type messageHandlerServiceType)
        {
            var t = typeof(IEnumerable<>).MakeGenericType(messageHandlerServiceType);
            return (IEnumerable<object>) _scope.Resolve(t);
        }

        public IEnumerable<T> ResolveDependency<T>()
        {
            return _scope.Resolve<IEnumerable<T>>();
        }
    }
}