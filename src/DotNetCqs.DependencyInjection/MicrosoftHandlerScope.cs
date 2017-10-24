using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCqs.DependencyInjection.Microsoft
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

        public IEnumerable<object> ResolveAll(Type messageHandlerType)
        {
            return _scope.ServiceProvider.GetServices(messageHandlerType);
        }

        public object Resolve(Type concreteHandlerType)
        {
            return _scope.ServiceProvider.GetService(concreteHandlerType);
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