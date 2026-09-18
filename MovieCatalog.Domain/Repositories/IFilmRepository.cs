using MovieCatalog.Domain.Entity;

namespace MovieCatalog.Domain.Repositories;

public interface IFilmRepository : IRepository<FilmEntity>
{
    Task<bool> ExistsByTitleAsync(string title, string? excludeId = null);
}
