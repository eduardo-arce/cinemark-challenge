using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MovieCatalog.Infra.Settings;

namespace MovieCatalog.Infra.Persistence;

public sealed class MongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        Database = client.GetDatabase(settings.Value.DatabaseName);
    }
}
