using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace DotNetCqs.Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterQueryHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsQueryHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterCommandHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsCommandHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterApplicationEventHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsEventHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterRequestReplyHandlers(this ContainerBuilder builer, params Assembly[] assemblies)
        {
            return
                builer.RegisterAssemblyTypes(assemblies)
                    .Where(IsRequestReplyHandler)
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope()
                    .OwnedByLifetimeScope();
        }

        public static void RegisterCqsBus(this ContainerBuilder builder, ICqsStorage storage)
        {
            builder.RegisterType<ContainerCommandBus>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ContainerQueryBus>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ContainerRequestReplyBus>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ContainerEventBus>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(storage).AsImplementedInterfaces().SingleInstance();
        }

        private static bool IsQueryHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IApplicationEventSubscriber<>));
        }

        private static bool IsCommandHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
        }

        private static bool IsEventHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
        }

        private static bool IsRequestReplyHandler(Type arg)
        {
            return arg.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
        }
    }
}
