using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;

namespace MovieCatalog.Application.Mapper
{
    public static class FilmMapperExtensions
    {
        public static FilmEntity ConvertFilmCreateDTOToEntity(FilmCreateDTO dto)
        {
            return new FilmEntity
            {
                Title = dto.Title.Trim(),
                Synopsis = dto.Synopsis?.Trim(),
                Genre = dto.Genre,
                ReleaseDate = dto.ReleaseDate.Date,
                DurationMinutes = dto.DurationMinutes,
                Rating = dto.Rating,
                Active = dto.Active
            };
        }
          
        public static FilmEntity ConvertFilmUpdateDTOToEntity(string id, FilmUpdateDTO dto)
        {
            return new FilmEntity
            {
                Id = id,
                Title = dto.Title.Trim(),
                Synopsis = dto.Synopsis?.Trim(),
                Genre = dto.Genre,
                ReleaseDate = dto.ReleaseDate.Date,
                DurationMinutes = dto.DurationMinutes,
                Rating = dto.Rating,
                Active = dto.Active
            };
        }
    }
}


