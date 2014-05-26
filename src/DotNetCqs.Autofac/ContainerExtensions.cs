using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace DotNetCqs.Autofac
{
    /// <summary>
    /// Autofac extensions
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Start all classes that implement <see cref="IStartable"/>
        /// </summary>
        /// <param name="container">container instance</param>
        public static void StartServices(this IContainer container)
        {
            var services = container.Resolve<IEnumerable<IStartable>>();
            foreach (var service in services)
            {
                service.Start();
            }
        }

        /// <summary>
        /// Stop all classes that implement <see cref="IStartable"/>
        /// </summary>
        /// <param name="container">container instance</param>
        public static void StopServices(this IContainer container)
        {
            var services = container.Resolve<IEnumerable<IStartable>>();
            foreach (var service in services)
            {
                service.Stop();
            }
        }
    }
}
