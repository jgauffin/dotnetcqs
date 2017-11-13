using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DotNetCqs.Queues;
using Microsoft.ServiceBus.Messaging;

namespace DotNetCqs.Azure.ServiceBus
{
    public class BrokeredMessageConverter
    {
        private readonly IMessageSerializer<string> _messageSerializer;

        public BrokeredMessageConverter(IMessageSerializer<string> messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public Tuple<ClaimsPrincipal, Message> FromBrokeredMessage(BrokeredMessage brokeredMessage)
        {
            var body = _messageSerializer.Deserialize(brokeredMessage.ContentType, brokeredMessage.GetBody<string>());

            var props = new Dictionary<string, string>();
            foreach (var kvp in brokeredMessage.Properties)
                props.Add(kvp.Key, kvp.Value.ToString());

            ClaimsPrincipal principal = null;
            if (props.TryGetValue("X-ClaimsType", out var claimsType))
            {
                var claimsStr = props["X-Claims"];
                props.Remove("X-Claims");
                props.Remove("X-ClaimsType");

                var claimDtos = (IEnumerable<ClaimDto>) _messageSerializer.Deserialize(claimsType, claimsStr);
                var claims = claimDtos.Select(x => x.ToClaim()).ToList();
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            }

            var msg = new Message(body, props)
            {
                CorrelationId = brokeredMessage.CorrelationId != null
                    ? Guid.Parse(brokeredMessage.CorrelationId)
                    : Guid.Empty,
                MessageId = Guid.Parse(brokeredMessage.MessageId),
                ReplyQueue = brokeredMessage.ReplyTo
            };

            return new Tuple<ClaimsPrincipal, Message>(principal, msg);
        }

        public BrokeredMessage ToBrokeredMessage(Message message, ClaimsPrincipal principal)
        {
            _messageSerializer.Serialize(message.Body, out var dtoStr, out var contentType);

            if (message.MessageId == Guid.Empty)
                message.MessageId = Guid.NewGuid();

            var msg = new BrokeredMessage(dtoStr)
            {
                MessageId = message.MessageId.ToString(),
                ContentType = contentType,
                ReplyTo = message.ReplyQueue
            };

            if (message.CorrelationId != Guid.Empty)
                msg.CorrelationId = message.CorrelationId.ToString();

            foreach (var kvp in message.Properties)
                msg.Properties[kvp.Key] = kvp.Value;

            if (principal != null)
            {
                var claims = principal.Claims.Select(x => new ClaimDto(x)).ToList();
                _messageSerializer.Serialize(claims, out var claimsStr, out var claimsContentType);
                msg.Properties["X-Claims"] = claimsStr;
                msg.Properties["X-Claims-Type"] = claimsContentType;
            }

            return msg;
        }
    }
}