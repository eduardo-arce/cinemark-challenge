using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Domain.IUseCase.Account
{
    public interface IFilmDelete
    {
        Task<Result<FilmEntity>> DeleteAsync(string filmId);
    }
}
