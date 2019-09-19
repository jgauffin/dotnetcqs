using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs.DependencyInjection;
using DotNetCqs.MessageProcessor;
using DotNetCqs.Serializer.Newtonsoft.Json;
using DotNetCqs.Tests.MessageProcessor;
using Microsoft.Extensions.Configuration;

namespace DotNetCqs.Queues.Azure.ServiceBus.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            //var conStr = config["ConnectionString"];
            //var queueName = config["QueueName"];
            var conStr =
                "Endpoint=sb://coderrlive.servicebus.windows.net/;SharedAccessKeyName=App;SharedAccessKey=zV623X84oZuyWEgn/PR21a72/snkxZA/r7PIMJYWEbY=";
            var queueName = "labb";

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, "11"),
                new Claim(ClaimTypes.Name, "Arne"),
                new Claim(ClaimTypes.Email, "Jonas@gauffin.com")
            };
            var identity = new ClaimsIdentity(claims, "Mofo");
            var p = new ClaimsPrincipal(identity);
            var queue = new AzureMessageQueue(conStr, queueName) { MessageSerializer = new JsonMessageSerializer() };
            ClearQueue(queue);

            Console.WriteLine("Sending");
            using (var session = queue.BeginSession())
            {
                session.EnqueueAsync(p, new Message("Hello world!"));
                session.SaveChanges();
            }
            Console.WriteLine("SENT");

            QueueListener listener = new QueueListener(queue, queue, new ManualScopeFactory());
            listener.RunAsync(new CancellationToken()).GetAwaiter().GetResult();

            SendReceiveSingle(queue);
            Console.WriteLine("==============");
            SendReceiveBatch(queue);
            Console.WriteLine("==============");
            ReceiveFailure(queue);
            Console.WriteLine("==============");

            Console.WriteLine("Done, press ENTER to quit");
            Console.ReadLine();
        }

        private static void ClearQueue(AzureMessageQueue queue)
        {
            using (var session = queue.BeginSession())
            {
                while (true)
                {
                    var message = session.Dequeue(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
                    Console.WriteLine("CLEAR DQ: " + message?.Body);
                    if (message == null)
                        break;
                }

                session.SaveChanges().GetAwaiter().GetResult();
            }
        }

        private static void SendReceiveSingle(AzureMessageQueue queue)
        {
            using (var session = queue.BeginSession())
            {
                Console.WriteLine("Sending");
                session.EnqueueAsync(new Message("Hello world")).GetAwaiter().GetResult();
                session.SaveChanges().GetAwaiter().GetResult();
            }

            using (var session = queue.BeginSession())
            {
                var message = session.Dequeue(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
                Console.WriteLine("GOT: " + message.Body);
                session.SaveChanges().GetAwaiter().GetResult();
            }
        }

        private static void SendReceiveBatch(AzureMessageQueue queue)
        {
            using (var session = queue.BeginSession())
            {
                Console.WriteLine("Sending");
                session.EnqueueAsync(new Message("Hello world1")).GetAwaiter().GetResult();
                session.EnqueueAsync(new Message("Hello world2")).GetAwaiter().GetResult();
                session.EnqueueAsync(new Message("Hello world3")).GetAwaiter().GetResult();
                session.EnqueueAsync(new Message("Hello world4")).GetAwaiter().GetResult();
                session.SaveChanges().GetAwaiter().GetResult();
            }

            using (var session = queue.BeginSession())
            {
                var message = session.Dequeue(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
                Console.WriteLine("GOT1: " + message.Body);
                message = session.Dequeue(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
                Console.WriteLine("GOT2: " + message.Body);
                message = session.Dequeue(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
                Console.WriteLine("GOT3: " + message.Body);
                message = session.Dequeue(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
                Console.WriteLine("GOT4: " + message.Body);
                session.SaveChanges().GetAwaiter().GetResult();
            }
        }

        private static void ReceiveFailure(AzureMessageQueue queue)
        {
            using (var session = queue.BeginSession())
            {
                Console.WriteLine("Sending");
                session.EnqueueAsync(new Message("Hello world FAIL")).GetAwaiter().GetResult();
                session.SaveChanges().GetAwaiter().GetResult();
            }

            for (int i = 0; i < 11; i++)
            {
                try
                {
                    using (var session = queue.BeginSession())
                    {
                        var message = session.Dequeue(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
                        if (message == null)
                            Console.WriteLine("** GOT NONE FROM FAIL, YAY!");
                        else
                            Console.WriteLine("GOT FAIL: " + message.Body);

                        throw new Exception("shit!");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {i}: {ex.Message}");
                }
            }

        }
    }

    internal class ManualScopeFactory : IHandlerScopeFactory
    {
        public IHandlerScope CreateScope()
        {
            return new ManualScope();
        }
    }

    public class MessageHandler : IMessageHandler<string>
    {
        public Task HandleAsync(IMessageContext context, string message)
        {
            Console.WriteLine("Got it: " + message + ", principal: " + context.Principal);
            return Task.CompletedTask;
        }
    }

    internal class ManualScope : IHandlerScope
    {
        public void Dispose()
        {
            
        }

        public IEnumerable<object> Create(Type messageHandlerServiceType)
        {
            Console.WriteLine("Creating "  + messageHandlerServiceType);
            return new List<object>() {new MessageHandler()};
        }

        public IEnumerable<T> ResolveDependency<T>()
        {
            if (typeof(T) == typeof(IMessageInvoker))
            {
                return new[] {(T) (object) new MessageInvoker(this)};
            }

            Console.WriteLine("Returning depndency " + typeof(T));
            return new T[0];
        }
    }
}
