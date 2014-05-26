using System;
using DotNetCqs;

namespace TestApp
{
public class ShipOrder : Command
{
    public ShipOrder(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentNullException("orderId");

        OrderId = orderId;
    }

    public Guid OrderId { get; private set; }
}
}