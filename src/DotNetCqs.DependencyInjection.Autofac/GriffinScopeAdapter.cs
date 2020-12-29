using System;
using System.Collections.Generic;
using Autofac;

namespace DotNetCqs.DependencyInjection.Autofac
{
    public class AutofacScopeAdapter : IHandlerScope
    {
        private readonly ILifetimeScope _scope;

        public AutofacScopeAdapter(ILifetimeScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public void Dispose()
        {
            _scope.Dispose();
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

        public override int GetHashCode()
        {
            return _scope.GetHashCode();
        }
    }
}