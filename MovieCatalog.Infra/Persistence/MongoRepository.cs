using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Repositories;

namespace MovieCatalog.Infra.Persistence
{
    public class MongoRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> Collection;

        public MongoRepository(MongoContext context, string collectionName)
        {
            Collection = context.Database.GetCollection<T>(collectionName);
        }

        private static FilterDefinition<T> NotDeleted =>
            Builders<T>.Filter.Eq(e => e.IsDeleted, false);

        public async Task<T?> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id) & NotDeleted;
            return await Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<(IReadOnlyList<T> Items, long TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>>? filter, int page, int pageSize)
        {
            var baseFilter = NotDeleted;
            if (filter is not null)
                baseFilter &= Builders<T>.Filter.Where(filter);

            var totalCount = await Collection.CountDocumentsAsync(baseFilter);

            var items = await Collection
                .Find(baseFilter)
                .SortByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(T entity)
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await Collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
            await Collection.ReplaceOneAsync(filter, entity);
        }
    }
}
