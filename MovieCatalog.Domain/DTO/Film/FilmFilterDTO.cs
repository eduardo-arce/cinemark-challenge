using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Domain.DTO.Film
{
    public class FilmFilterDTO
    {
        public Genre? Genre { get; set; }

        public bool? Active { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
