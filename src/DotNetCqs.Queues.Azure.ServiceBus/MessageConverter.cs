using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DotNetCqs.Queues.Azure.ServiceBus
{
    public class MessageConverter
    {
        private readonly IMessageSerializer<string> _messageSerializer;
        private readonly ConcurrentQueue<MemoryStream> _allocatedStream = new ConcurrentQueue<MemoryStream>();

        public MessageConverter(IMessageSerializer<string> messageSerializer)
        {
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        }

        public Tuple<ClaimsPrincipal, Message> FromAzureMessage(Microsoft.Azure.ServiceBus.Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message.UserProperties == null)
                throw new ArgumentException("message.UserProperties cannot be null.");
            if (message.Body == null)
                throw new ArgumentException("message.Body cannot be null.");

            string bodyStr;
            if (message.UserProperties.ContainsKey("IsCompressed"))
            {
                var ms = new MemoryStream(message.Body, 0, message.Body.Length);
                var zipStream = new GZipStream(ms, CompressionMode.Decompress);

                if (!_allocatedStream.TryDequeue(out var destinationStream))
                    destinationStream = new MemoryStream();

                zipStream.CopyTo(destinationStream);
                bodyStr = Encoding.UTF8.GetString(destinationStream.GetBuffer(), 0, (int)destinationStream.Length);
                destinationStream.SetLength(0);
                _allocatedStream.Enqueue(destinationStream);
                message.UserProperties.Remove("IsCompressed");
            }
            else
                bodyStr = Encoding.UTF8.GetString(message.Body);

            var body = _messageSerializer.Deserialize(message.ContentType, bodyStr);

            var props = new Dictionary<string, string>();
            foreach (var kvp in message.UserProperties)
                props.Add(kvp.Key, kvp.Value?.ToString());

            if (props.TryGetValue("X-Claims-Auth-Type", out var authType))
            {
                props.Remove("X-Claims-Auth-Type");
            }
            else
            {
                authType = "DotNetCqs";
            }

            ClaimsPrincipal principal = null;
            if (props.TryGetValue("X-Claims-Type", out var claimsType))
            {
                var claimsStr = props["X-Claims"];
                props.Remove("X-Claims");
                props.Remove("X-Claims-Type");

                var claimDtos = (IEnumerable<ClaimDto>)_messageSerializer.Deserialize(claimsType, claimsStr);
                var claims = claimDtos.Select(x => x.ToClaim()).ToList();
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authType));
            }

            

            var msg = new Message(body, props)
            {
                CorrelationId = message.CorrelationId == null ? Guid.Empty : Guid.Parse(message.CorrelationId),
                MessageId = Guid.Parse(message.MessageId),
                ReplyQueue = message.ReplyTo
            };

            return new Tuple<ClaimsPrincipal, Message>(principal, msg);
        }

        public Microsoft.Azure.ServiceBus.Message ToAzureMessage(Message message, ClaimsPrincipal principal)
        {
            _messageSerializer.Serialize(message.Body, out var dtoStr, out var contentType);
            if (message.MessageId == Guid.Empty)
                message.MessageId = Guid.NewGuid();

            var body = Encoding.UTF8.GetBytes(dtoStr);
            bool isCompressed = false;
            if (body.Length >= 256000)
            {
                if (!_allocatedStream.TryDequeue(out var destinationStream))
                    destinationStream = new MemoryStream();

                var zipStream = new GZipStream(destinationStream, CompressionLevel.Optimal);
                zipStream.Write(body, 0, body.Length);
                body = new byte[destinationStream.Length];
                if (body.Length > 256000)
                    throw new NotSupportedException("Message is larger than 256kb (even when its compressed).");

                Buffer.BlockCopy(destinationStream.GetBuffer(), 0, body, 0, (int)destinationStream.Length);
                _allocatedStream.Enqueue(destinationStream);
                isCompressed = true;
            }

            var msg = new Microsoft.Azure.ServiceBus.Message(body)
            {
                MessageId = message.MessageId.ToString(),
                ContentType = contentType,
                ReplyTo = message.ReplyQueue,
            };

            if (isCompressed)
                msg.UserProperties["IsCompressed"] = "gzip";

            if (message.CorrelationId != Guid.Empty)
                msg.CorrelationId = message.CorrelationId.ToString();

            foreach (var kvp in message.Properties)
                msg.UserProperties[kvp.Key] = kvp.Value;

            if (principal != null)
            {
                var claims = principal.Claims.Select(x => new ClaimDto(x)).ToList();
                _messageSerializer.Serialize(claims, out var claimsStr, out var claimsContentType);
                msg.UserProperties["X-Claims"] = claimsStr;
                msg.UserProperties["X-Claims-Type"] = claimsContentType;
                msg.UserProperties["X-Claims-Auth-Type"] = principal.Identity.AuthenticationType;
            }

            return msg;
        }
    }
}