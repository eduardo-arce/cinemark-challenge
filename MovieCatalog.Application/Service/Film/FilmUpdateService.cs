using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Application.Mapper;
using MovieCatalog.Application.Validators;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Events;
using MovieCatalog.Domain.IUseCase.Account;
using MovieCatalog.Domain.Repositories;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Application.Service.Film
{
    public class FilmUpdateService : IFilmUpdate
    {
        private const string CachePrefix = "films";
        private readonly IFilmRepository _repository;
        private readonly ICacheService _cache;
        private readonly IEventPublisher _eventPublisher;
        private readonly IValidator<FilmUpdateDTO> _updateValidator;
        private readonly ILogger<FilmUpdateService> _logger;

        public FilmUpdateService(IFilmRepository repository,
            ICacheService cache,
            IEventPublisher eventPublisher,
            IValidator<FilmUpdateDTO> updateValidator,
            ILogger<FilmUpdateService> logger)
        {
            _repository = repository;
            _cache = cache;
            _eventPublisher = eventPublisher;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<Result<FilmEntity>> UpdateAsync(string id, FilmUpdateDTO filmUpdateDTO)
        {
            var validation = await _updateValidator.ValidateAsync(filmUpdateDTO);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var filmById = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Filme com ID '{id}' não encontrado");

            if (await _repository.ExistsByTitleAsync(filmUpdateDTO.Title.Trim(), excludeId: id))
                throw new ValidationException("Já existe um filme com este título");

            var film = FilmMapperExtensions.ConvertFilmUpdateDTOToEntity(id, filmUpdateDTO);

            film.SetUpdatedAt();

            await _repository.UpdateAsync(film);

            _logger.LogInformation("Film updated: {FilmId} - {Title}", film.Id, film.Title);

            await _cache.RemoveAsync($"{CachePrefix}:byid:{id}");

            await _cache.RemoveByPrefixAsync($"{CachePrefix}:list");

            await _eventPublisher.PublishAsync(new FilmUpdatedEvent(film.Id, film.Title, film.Genre.ToString()));

            return Result<FilmEntity>.Ok(
                content: film,
                message: "film updated!!!"
            );
        }        
    }
}
