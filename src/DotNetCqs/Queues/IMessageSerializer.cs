namespace DotNetCqs.Queues
{
    public interface IMessageSerializer<TDestinationType>
    {
        /// <summary>
        /// </summary>
        /// <param name="contentType">
        ///     "Message" for <see cref="Message" />, otherwise the type that was specified in
        ///     <see cref="Serialize" />.
        /// </param>
        /// <param name="serializedDto">Serialized object, either a <see cref="Message" /> or the transported CQS object</param>
        /// <returns></returns>
        /// <exception cref="SerializationException">Failed to deserialize message</exception>
        object Deserialize(string contentType, TDestinationType serializedDto);

        /// <summary>
        ///     Serialize outbound message
        /// </summary>
        /// <param name="dto">Object to serialize, either a <see cref="Message" /> or the transported CQS object.</param>
        /// <param name="serializedDto">serialized message</param>
        /// <param name="contentType">Implementation specific type used to be able to <see cref="Deserialize" /> messages.</param>
        /// <exception cref="SerializationException">Failed to serialize message</exception>
        void Serialize(object dto, out TDestinationType serializedDto, out string contentType);
    }
}