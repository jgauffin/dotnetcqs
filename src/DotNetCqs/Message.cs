using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCqs
{
    /// <summary>
    ///     Messages wrapped for transport and handling within the message system
    /// </summary>
    public class Message
    {
        /// <summary>
        ///     Create a new instance of <see cref="Message" />.
        /// </summary>
        /// <param name="body">Actual message</param>
        /// <exception cref="ArgumentNullException">body</exception>
        /// <exception cref="ArgumentException">Tried to wrap a <see cref="Message" /> in a <see cref="Message" /></exception>
        public Message(object body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (body is Message)
                throw new ArgumentException("Cannot wrap a Message in a Message, (inner type: " +
                                            ((Message)body).Body + ").");

            MessageId = GuidFactory.Create();
            Body = body;
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Create a new instance of <see cref="Message" />.
        /// </summary>
        /// <param name="body">Actual message</param>
        /// <param name="properties">Properties to attach</param>
        /// <exception cref="ArgumentNullException">body</exception>
        /// <exception cref="ArgumentException">Tried to wrap a <see cref="Message" /> in a <see cref="Message" /></exception>
        public Message(object body, IDictionary<string, string> properties)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (body is Message)
                throw new ArgumentException("Cannot wrap a Message in a Message, (inner type: " +
                                            ((Message)body).Body + ").");

            MessageId = GuidFactory.Create();
            Body = body;
            Properties = properties ?? new Dictionary<string, string>();
        }

        protected Message()
        {
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Actual message
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        ///     Message that the current message relates to (if set, the current message is typically a reply)
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     Identity of the inbound message
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        ///     Properties (which you can attach yourself).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Can be used to transport additional information to the remote endpoint or to attach information
        ///         required during serialization or transportation.
        ///     </para>
        ///     <para>All non-application specific properties should start with <c>X-</c>.</para>
        /// </remarks>
        public IDictionary<string, string> Properties { get; private set; }


        /// <summary>
        ///     Path to the reply queue.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Format depends on the queue implementation
        ///     </para>
        ///     <para>
        ///         Used when a message requires a reply. Also set for replies (indicated by <see cref="CorrelationId" />) to tell
        ///         which queue the reply should be delivered to.
        ///     </para>
        /// </remarks>
        public string ReplyQueue { get; set; }

        /// <summary>
        ///     Create a new reply.
        /// </summary>
        /// <param name="correlationId">Message id of the message that we are replying to.</param>
        /// <param name="reply">Reply DTO</param>
        /// <returns>message to send</returns>
        public static Message CreateReply(Guid correlationId, object reply)
        {
            return new Message
            {
                CorrelationId = correlationId,
                Body = reply
            };
        }

        /// <summary>
        ///     Create a new reply (typically for queries, we did not find the requested item).
        /// </summary>
        /// <param name="correlationId">Message id of the message that we are replying to.</param>
        /// <returns>message to send</returns>
        public static Message CreateReply(Guid correlationId)
        {
            return new Message
            {
                CorrelationId = correlationId
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {

            var props = Properties != null
                ? string.Join(",", Properties.Select(x => $"{x.Key}={x.Value}"))
                : "";
            return props == ""
                ? $"id: {MessageId:N}, body: {Body}"
                : $"id: {MessageId:N}, props: ({props}), body: {Body}";
        }
    }
}