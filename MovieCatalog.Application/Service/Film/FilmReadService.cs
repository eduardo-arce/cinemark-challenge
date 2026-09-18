using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.IUseCase.Account;
using MovieCatalog.Domain.Repositories;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Application.Service.Film
{
    public class FilmReadService : IFilmRead
    {
        private const int CacheTtlMinutes = 5;
        private const string CachePrefix = "films";
        private readonly IFilmRepository _repository;
        private readonly ICacheService _cache;

        public FilmReadService(
            IFilmRepository repository,
            ICacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }


        public async Task<Result<FilmEntity>> GetByIdAsync(string id)
        {
            var cacheKey = $"{CachePrefix}:byid:{id}";

            var cached = await _cache.GetAsync<FilmEntity>(cacheKey);

            if (cached is not null)
            {
                return Result<FilmEntity>.Ok(
                    content: cached,
                    message: "film searched!!!"
                );
            }

            var film = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Filme com ID '{id}' não encontrado");

            await _cache.SetAsync(cacheKey, film, TimeSpan.FromMinutes(CacheTtlMinutes));

            return Result<FilmEntity>.Ok(
                content: film,
                message: "film searched!!!"
            );
        }

        public async Task<Result<PaginatedResult<FilmEntity>>> GetAllAsync(FilmFilterDTO filmFilterDTO)
        {
            var cacheKey = $"{CachePrefix}:list:g={filmFilterDTO.Genre} | a={filmFilterDTO.Active} | p={filmFilterDTO.Page} | s={filmFilterDTO.PageSize}";

            var cached = await _cache.GetAsync<PaginatedResult<FilmEntity>>(cacheKey);

            if (cached is not null)
            {
                return Result<PaginatedResult<FilmEntity>>.Ok(
                    content: cached,
                    message: "film searched!!!"
                );
            }

            Expression<Func<FilmEntity, bool>>? filter = null;

            if (filmFilterDTO.Genre.HasValue && filmFilterDTO.Active.HasValue)
                filter = f => f.Genre == filmFilterDTO.Genre.Value && f.Active == filmFilterDTO.Active.Value;
            else if (filmFilterDTO.Genre.HasValue)
                filter = f => f.Genre == filmFilterDTO.Genre.Value;
            else if (filmFilterDTO.Active.HasValue)
                filter = f => f.Active == filmFilterDTO.Active.Value;

            var (items, totalCount) = await _repository.GetPagedAsync(filter, filmFilterDTO.Page, filmFilterDTO.PageSize);

            var response = new PaginatedResult<FilmEntity>
            {
                Page = filmFilterDTO.Page,
                PageSize = filmFilterDTO.PageSize,
                TotalItems = (int)totalCount,
                Items = items
            };

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(CacheTtlMinutes));

            return Result<PaginatedResult<FilmEntity>>.Ok(
                content: response,
                message: "film searched!!!"
            );
        }
    }
}
