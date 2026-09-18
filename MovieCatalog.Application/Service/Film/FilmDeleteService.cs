using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Events;
using MovieCatalog.Domain.IUseCase.Account;
using MovieCatalog.Domain.Repositories;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Application.Service.Film
{
    public class FilmDeleteService : IFilmDelete
    {
        private const string CachePrefix = "films";
        private readonly IFilmRepository _repository;
        private readonly ICacheService _cache;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<FilmDeleteService> _logger;

        public FilmDeleteService(IFilmRepository repository,
            ICacheService cache,
            IEventPublisher eventPublisher,
            ILogger<FilmDeleteService> logger)
        {
            _repository = repository;
            _cache = cache;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<Result<FilmEntity>> DeleteAsync(string id)
        {
            var film = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Filme com ID '{id}' não encontrado");

            film.MarkAsDeleted();

            await _repository.UpdateAsync(film);

            _logger.LogInformation("Film soft-deleted: {FilmId} - {Title}", film.Id, film.Title);

            await _cache.RemoveAsync($"{CachePrefix}:byid:{id}");

            await _cache.RemoveByPrefixAsync($"{CachePrefix}:list");

            await _eventPublisher.PublishAsync(new FilmDeletedEvent(film.Id, film.Title));

            return Result<FilmEntity>.Ok(
                content: film,
                message: "film deleted!!!"
            );
        }
    }
}
