using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DotNetCqs.Queues.AdoNet
{
    public class AdoNetMessageDto
    {
        public AdoNetMessageDto(ClaimsPrincipal principal, Message message, IMessageSerializer<string> serializer)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            Properties = message.Properties ?? new Dictionary<string, string>();

            Properties.Add("X-MessageId", message.MessageId.ToString());
            if (message.CorrelationId != Guid.Empty)
                Properties.Add("X-CorrelationId", message.CorrelationId.ToString());

            serializer.Serialize(message.Body, out var bodyStr, out var contentType);
            Body = bodyStr;
            Properties.Add("X-ContentType", contentType);

            Claims = principal != null
                ? principal.Claims.Select(x => new ClaimDto(x)).ToList()
                : new List<ClaimDto>();
        }

        protected AdoNetMessageDto()
        {
        }

        public string Body { get; set; }

        public List<ClaimDto> Claims { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public void ToMessage(IMessageSerializer<string> serializer, out Message message, out ClaimsPrincipal principal)
        {
            var contenType = Properties["X-ContentType"];

            var body = serializer.Deserialize(contenType, Body);
            var props = new Dictionary<string, string>(Properties);
            var claims = Claims.Select(x => x.ToClaim()).ToList();

            principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            message = new Message(body, props)
            {
                MessageId = Guid.Parse(props["X-MessageId"])
            };

            if (props.TryGetValue("X-CorrelationId", out var correlationId))
            {
                message.CorrelationId = Guid.Parse(correlationId);
                props.Remove("X-CorrelationId");
            }

            props.Remove("X-ContentType");
            props.Remove("X-MessageId");
        }
    }
}