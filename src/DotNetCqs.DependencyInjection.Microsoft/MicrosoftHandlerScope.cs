using System;
using System.Collections.Generic;
using DotNetCqs.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCqs
{
    public class MicrosoftHandlerScope : IHandlerScope
    {
        private readonly IServiceScope _scope;

        public MicrosoftHandlerScope(IServiceScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public IEnumerable<object> Create(Type messageHandlerServiceType)
        {
            return _scope.ServiceProvider.GetServices(messageHandlerServiceType);
        }

        public IEnumerable<T> ResolveDependency<T>()
        {
            return _scope.ServiceProvider.GetServices<T>();
        }

        public object Resolve(Type concreteHandlerType)
        {
            return _scope.ServiceProvider.GetService(concreteHandlerType);
        }

        public IEnumerable<object> ResolveAll(Type messageHandlerType)
        {
            return _scope.ServiceProvider.GetServices(messageHandlerType);
        }
    }
}