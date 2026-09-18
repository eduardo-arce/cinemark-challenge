using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MovieCatalog.Application.Service.Film;
using MovieCatalog.Domain.IUseCase.Account;

namespace MovieCatalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFilmCreate, FilmCreateService>();
        services.AddScoped<IFilmRead, FilmReadService>();
        services.AddScoped<IFilmUpdate, FilmUpdateService>();
        services.AddScoped<IFilmDelete, FilmDeleteService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
