using System;
using System.Threading.Tasks;
using DotNetCqs;

namespace TestApp
{
    public class ShipOrderHandler : ICommandHandler<ShipOrder>
    {
        public async Task ExecuteAsync(ShipOrder command)
        {
            Console.WriteLine("shipping " + command);
        }
    }

}