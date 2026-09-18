using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Domain.IUseCase.Account
{
    public interface IFilmCreate
    {
        Task<Result<FilmEntity>> CreateAsync(FilmCreateDTO filmCreateDTO);
    }
}
