using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers;
using Xunit;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests
{
    public class AdoNetMessageDTOTests
    {
        [Fact]
        public void Test()
        {
            var claims = new List<Claim>(new[] { new Claim(ClaimTypes.Name, "Arne") });
            var p = new ClaimsPrincipal(new ClaimsIdentity(claims, "Mine"));
            var msg = new Message("Hello");
            msg.Properties["X-MessageId"] = "abbas";
            msg.Properties["X-CorrelationId"] = "mucks";

            var sut = new AdoNetMessageDto(p, msg, new JsonSerializer());

        }
    }
}
