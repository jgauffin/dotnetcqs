using System.Collections.Generic;
using System.Data;

namespace DotNetCqs.Queues.AdoNet.IntegrationTests.Helpers
{
    internal static class DbExtensions
    {
        public static Dictionary<string, object> QueryRow(this IDbConnection connection, string sql)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return new Dictionary<string, object>();

                    var items = new Dictionary<string, object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                        items[reader.GetName(i)] = reader[i];
                    return items;
                }
            }
        }
    }
}