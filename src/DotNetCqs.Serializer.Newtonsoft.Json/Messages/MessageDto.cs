using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCqs.Serializer.Newtonsoft.Json.Messages
{
    public class MessageDto
    {
        public string Body { get; set; }
        public IdentityDto Identity { get; set; }
        public Dictionary<string,string> Properties { get; set; }
        public string MessageId { get; set; }
        
    }
}
