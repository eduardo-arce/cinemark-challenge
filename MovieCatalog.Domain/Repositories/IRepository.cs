using System.Linq.Expressions;
using MovieCatalog.Domain.Entity;

namespace MovieCatalog.Domain.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id);

    Task<(IReadOnlyList<T> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        int page,
        int pageSize);

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);
}
