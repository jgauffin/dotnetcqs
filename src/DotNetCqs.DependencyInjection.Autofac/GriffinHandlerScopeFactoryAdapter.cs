using System;
using Autofac;

namespace DotNetCqs.DependencyInjection.Autofac
{
    public class AutofacHandlerScopeFactory : IHandlerScopeFactory
    {
        private readonly IContainer _container;

        public AutofacHandlerScopeFactory(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IHandlerScope CreateScope()
        {
            var scope = _container.BeginLifetimeScope();
            return new AutofacScopeAdapter(scope);
        }
    }
}
