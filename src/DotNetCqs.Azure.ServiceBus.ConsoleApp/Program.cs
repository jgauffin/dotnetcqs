using System;
using DotNetCqs.Serializer.Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace DotNetCqs.Azure.ServiceBus.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var conStr = config["ConnectionString"];
            var queueName = config["QueueName"];

            var queue = new AzureMessageQueue(conStr, queueName) { MessageSerializer = new JsonMessageSerializer() };
            ClearQueue(queue);

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
}
