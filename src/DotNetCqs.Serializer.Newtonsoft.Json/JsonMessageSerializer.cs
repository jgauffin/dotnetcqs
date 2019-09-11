using System;
using DotNetCqs.Queues;
using DotNetCqs.Serializer.Newtonsoft.Json.Messages;
using Newtonsoft.Json;

namespace DotNetCqs.Serializer.Newtonsoft.Json
{
    public class JsonMessageSerializer : IMessageSerializer<string>
    {
        private JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new PrivateSetterContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto
        };

        public object Deserialize(string contentType, string serializedDto)
        {
            try
            {
                var type = contentType == "Message"
                    ? typeof(MessageDto)
                    : Type.GetType(contentType);

                if (type == null)
                    throw new SerializationException($"Failed to lookup type \'{contentType}\'.", serializedDto);

                return JsonConvert.DeserializeObject(serializedDto, type, _settings);
            }
            catch (JsonException ex)
            {
                throw new SerializationException($"Failed to deserialize \'{contentType}\'.", serializedDto, ex);
            }
        }

        public void Serialize(object dto, out string serializedDto, out string contentType)
        {
            try
            {
                contentType = dto.GetType().AssemblyQualifiedName;
                serializedDto = JsonConvert.SerializeObject(dto, typeof(MessageDto), _settings);
            }
            catch (JsonException ex)
            {
                throw new SerializationException($"Failed to serialize \'{dto.GetType().FullName}\'.", dto, ex);
            }
        }
    }
}
