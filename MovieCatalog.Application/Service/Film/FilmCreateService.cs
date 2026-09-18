using System.Threading;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Application.Mapper;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Events;
using MovieCatalog.Domain.IUseCase.Account;
using MovieCatalog.Domain.Repositories;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Application.Service.Film
{
    public class FilmCreateService : IFilmCreate
    {
        private const string CachePrefix = "films";
        private readonly IFilmRepository _repository;
        private readonly ICacheService _cache;
        private readonly IEventPublisher _eventPublisher;
        private readonly IValidator<FilmCreateDTO> _createValidator;
        private readonly ILogger<FilmCreateService> _logger;

        public FilmCreateService(IFilmRepository repository,
            ICacheService cache,
            IEventPublisher eventPublisher,
            IValidator<FilmCreateDTO> createValidator,
            ILogger<FilmCreateService> logger)
        {
            _repository = repository;
            _cache = cache;
            _eventPublisher = eventPublisher;
            _createValidator = createValidator;
            _logger = logger;
        }

        public async Task<Result<FilmEntity>> CreateAsync(FilmCreateDTO filmCreateDTO)
        {
            var validation = await _createValidator.ValidateAsync(filmCreateDTO);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            if (await _repository.ExistsByTitleAsync(filmCreateDTO.Title.Trim()))
                throw new ValidationException("Já existe um filme com este título");

            var film = FilmMapperExtensions.ConvertFilmCreateDTOToEntity(filmCreateDTO);

            await _repository.AddAsync(film);

            _logger.LogInformation("Film created: {FilmId} - {Title}", film.Id, film.Title);

            await _cache.RemoveByPrefixAsync($"{CachePrefix}:list");

            await _eventPublisher.PublishAsync(new FilmCreatedEvent(film.Id, film.Title, film.Genre.ToString(), film.ReleaseDate));

            return Result<FilmEntity>.Created(
                content: film,
                message: "film created!!!"
            );
        }
    }
}
