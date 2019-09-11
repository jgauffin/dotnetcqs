using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetCqs.Serializer.Newtonsoft.Json
{
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (prop.Writable)
                return prop;

            var property = member as System.Reflection.PropertyInfo;
            if (property == null)
                return prop;

            var hasPrivateSetter = property.GetSetMethod(true) != null;
            prop.Writable = hasPrivateSetter;
            return prop;
        }
    }
}