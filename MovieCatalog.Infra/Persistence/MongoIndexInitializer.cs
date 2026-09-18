using MongoDB.Driver;
using MovieCatalog.Domain.Entity;

namespace MovieCatalog.Infra.Persistence;

public sealed class MongoIndexInitializer
{
    private readonly MongoContext _context;

    public MongoIndexInitializer(MongoContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        var collection = _context.Database.GetCollection<FilmEntity>("films");

        var indexes = new List<CreateIndexModel<FilmEntity>>
        {
            new(Builders<FilmEntity>.IndexKeys.Ascending(f => f.Genre)),
            new(Builders<FilmEntity>.IndexKeys.Ascending(f => f.Active)),
            new(Builders<FilmEntity>.IndexKeys.Ascending(f => f.IsDeleted)),
            new(
                Builders<FilmEntity>.IndexKeys.Ascending(f => f.Title),
                new CreateIndexOptions { Unique = true, Collation = new Collation("en", strength: CollationStrength.Secondary) })
        };

        await collection.Indexes.CreateManyAsync(indexes);
    }
}
