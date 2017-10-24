using System;
using System.Linq;
using System.Reflection;
using DotNetCqs.DependencyInjection;
using DotNetCqs.DependencyInjection.GriffinContainer;
using Griffin.Container;

namespace DotNetCqs.Tests.Helpers
{
    public static class TestHelpers
    {
      
        public static void RegisterHandler(this ContainerRegistrar registrar, object handler)
        {
            registrar.RegisterInstance(handler.GetType(), handler);
            var reg = registrar.Registrations.First(x => x.Services.Any(y => y == handler.GetType()));
            var bajs = handler.GetType().GetInterfaces();
            foreach (var b in bajs)
            {
                if (b.IsConstructedGenericType)
                    reg.AddService(b);
            }
            
        }

        public static bool IsQuery(this object cqsObject)
        {
            var baseType = cqsObject.GetType().BaseType;
            while (baseType != null)
            {
                if (baseType.FullName.StartsWith("DotNetCqs.Query"))
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
    }
}