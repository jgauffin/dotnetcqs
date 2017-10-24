using System.Data;

namespace DotNetCqs.Queues.AdoNet
{
    public static class CommandExtensions
    {
        public static IDataParameter AddParameter(this IDbCommand command, string name, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
            return p;
        }
    }
}