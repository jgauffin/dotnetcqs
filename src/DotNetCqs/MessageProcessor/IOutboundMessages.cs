using System.Collections.Generic;

namespace DotNetCqs.MessageProcessor
{
    public interface IOutboundMessages: IMessageContext
    {
        IList<Message> OutboundMessages { get; }
        IList<Message> Replies { get; }
    }
}