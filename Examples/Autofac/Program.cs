using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using DotNetCqs;
using DotNetCqs.Autofac;
using DotNetCqs.Autofac.Storage;

namespace TestApp
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            CreateContainer();
            _container.StartServices();
            var commandBus = _container.Resolve<ICommandBus>();

            var cmd = new ShipOrder(Guid.NewGuid());
            commandBus.ExecuteAsync(cmd).Wait();

            Console.ReadLine();
            _container.StopServices();
        }

        private static void CreateContainer()
        {
            var cb = new ContainerBuilder();

            // 
            cb.RegisterCqsBus(new CqsFileStorage("Data\\"));
            cb.Register(x => _container).As<IContainer>().SingleInstance();
            cb.RegisterCommandHandlers(Assembly.GetExecutingAssembly());
            _container = cb.Build();
        }
    }
}
