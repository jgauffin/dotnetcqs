ADO.NET based queues for DotNetCqs
==================================

Create the following table:

CREATE TABLE MessageQueue 
( 
    Id int not null identity primary key,
    QueueName varchar(40) not null,
    CreatedAtUtc datetime not null,
    MessageType varchar(512) not null,
    Body nvarchar(MAX) not null
);

Then configure the MessageQueueProvider:

var provider = new AdoNetMessageQueueProvider(OpenConnection, new JsonMessageQueueSerializer());

The connection method could look like this:

public IDbConnection OpenConnection()
{
    var conStr = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

    var con = new SqlConnection(conStr),
    con.Open();

    return con;
}

If you want to use another table name, change it:

provider.TableName = "MyTable";


Extras
========

You can create a serializer based on JSON.NET like this:

public class JsonMessageQueueSerializer : IMessageSerializer
{
    private JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ContractResolver = new IncludeNonPublicMembersContractResolver()
    };

    public object Deserialize(string contentType, string serializedDto)
    {
        try
        {
            var type = contentType == "Message"
                ? typeof(AdoNetMessageDto)
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
            serializedDto = JsonConvert.SerializeObject(dto);
        }
        catch (JsonException ex)
        {
            throw new SerializationException($"Failed to serialize \'{dto.GetType().FullName}\'.", dto, ex);
        }
    }
}


Important Gotchas
=================

* Keep number of queues per table low, or you'll get deadlocks on the table.
* Only use one QueueListener per queue.
