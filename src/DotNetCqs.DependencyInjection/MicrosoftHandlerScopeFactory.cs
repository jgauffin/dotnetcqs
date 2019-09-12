using System;
using DotNetCqs.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCqs
{
    public class MicrosoftHandlerScopeFactory : IHandlerScopeFactory
    {
        private readonly IServiceProvider _provider;

        public MicrosoftHandlerScopeFactory(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IHandlerScope CreateScope()
        {
            var scope = _provider.CreateScope();
            return new MicrosoftHandlerScope(scope);
        }
    }
}