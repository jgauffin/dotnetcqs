using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers
{
    public static class DictionaryExtensions
    {
        public static object Get(this IDictionary<string, object> instance, string name)
        {
            if (!instance.TryGetValue(name, out object value))
                throw new ArgumentException("Failed to find key '" + name + "', items: " +
                                            string.Join(", ", instance.Select(x => x.Key + "=" + x.Value)));
            return value;
        }
    }
}
