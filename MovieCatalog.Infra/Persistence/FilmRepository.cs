using MongoDB.Driver;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Repositories;

namespace MovieCatalog.Infra.Persistence
{
    public sealed class FilmRepository : MongoRepository<FilmEntity>, IFilmRepository
    {
        public FilmRepository(MongoContext context) : base(context, "films") { }

        public async Task<bool> ExistsByTitleAsync(
            string title, string? excludeId = null)
        {
            var filter = Builders<FilmEntity>.Filter.Regex(
                f => f.Title,
                new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(title.Trim())}$", "i"))
                & Builders<FilmEntity>.Filter.Eq(f => f.IsDeleted, false);

            if (excludeId is not null)
                filter &= Builders<FilmEntity>.Filter.Ne(f => f.Id, excludeId);

            return await Collection.Find(filter).AnyAsync();
        }
    }
}
