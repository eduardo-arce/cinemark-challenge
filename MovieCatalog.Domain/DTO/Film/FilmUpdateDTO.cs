using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Domain.DTO.Film
{
    public class FilmUpdateDTO
    {
        public string Title { get; set; } = null!;

        public string? Synopsis { get; set; }

        public Genre Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int DurationMinutes { get; set; }

        public decimal Rating { get; set; }

        public bool Active { get; set; }
    }
}
