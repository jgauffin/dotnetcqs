using System;
using Griffin.Container;
using DotNetCqs.DependencyInjection;

namespace DotNetCqs.DependencyInjection.GriffinContainer
{
    public class GriffinHandlerScopeFactoryAdapter : IHandlerScopeFactory
    {
        private readonly IParentContainer _container;

        public GriffinHandlerScopeFactoryAdapter(IParentContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IHandlerScope CreateScope()
        {
            var scope = _container.CreateChildContainer();
            return new GriffinScopeAdapter(scope);
        }
    }
}
