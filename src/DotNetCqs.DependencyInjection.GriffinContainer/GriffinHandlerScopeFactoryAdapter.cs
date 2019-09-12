using System;
using DotNetCqs.DependencyInjection;
using Griffin.Container;

namespace DotNetCqs
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
