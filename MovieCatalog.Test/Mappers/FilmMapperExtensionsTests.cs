using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MovieCatalog.Application.Mapper;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Test.Mappers
{
    [TestFixture]
    public class FilmMapperExtensionsTests
    {
        [Test]
        public void ConvertFilmCreateDTOToEntity_Should_Map_All_Fields()
        {
            var dto = new FilmCreateDTO
            {
                Title = "  Test Film  ",
                Synopsis = "  A synopsis  ",
                Genre = Genre.SciFi,
                ReleaseDate = new DateTime(2024, 3, 15, 10, 30, 0),
                DurationMinutes = 150,
                Rating = 9.0m,
                Active = true
            };

            var entity = FilmMapperExtensions.ConvertFilmCreateDTOToEntity(dto);

            entity.Title.Should().Be("Test Film");
            entity.Synopsis?.Should().Be("A synopsis");
            entity.Genre.Should().Be(Genre.SciFi);
            entity.ReleaseDate.Should().Be(new DateTime(2024, 3, 15));
            entity.DurationMinutes.Should().Be(150);
            entity.Rating.Should().Be(9.0m);
            entity.Active.Should().BeTrue();
        }

        [Test]
        public void ConvertFilmCreateDTOToEntity_Should_Handle_Null_Synopsis()
        {
            var dto = new FilmCreateDTO
            {
                Title = "Title",
                Synopsis = null,
                Genre = Genre.Action,
                ReleaseDate = new DateTime(2024, 1, 1),
                DurationMinutes = 100,
                Rating = 7.0m,
                Active = true
            };

            var entity = FilmMapperExtensions.ConvertFilmCreateDTOToEntity(dto);

            entity.Synopsis?.Should().BeNull();
        }

        [Test]
        public void ConvertFilmCreateDTOToEntity_Should_Strip_Time_From_ReleaseDate()
        {
            var dto = new FilmCreateDTO
            {
                Title = "Title",
                Genre = Genre.Drama,
                ReleaseDate = new DateTime(2024, 6, 15, 14, 30, 45),
                DurationMinutes = 100,
                Rating = 7.0m
            };

            var entity = FilmMapperExtensions.ConvertFilmCreateDTOToEntity(dto);

            entity.ReleaseDate.Hour.Should().Be(0);
            entity.ReleaseDate.Minute.Should().Be(0);
        }

        [Test]
        public void ConvertFilmUpdateDTOToEntity_Should_Map_All_Fields_With_Id()
        {
            var dto = new FilmUpdateDTO
            {
                Title = "  Updated Title  ",
                Synopsis = "  Updated synopsis  ",
                Genre = Genre.Comedy,
                ReleaseDate = new DateTime(2024, 12, 25, 20, 0, 0),
                DurationMinutes = 90,
                Rating = 6.5m,
                Active = false
            };

            var entity = FilmMapperExtensions.ConvertFilmUpdateDTOToEntity("myid", dto);

            entity.Id.Should().Be("myid");
            entity.Title.Should().Be("Updated Title");
            entity.Synopsis?.Should().Be("Updated synopsis");
            entity.Genre.Should().Be(Genre.Comedy);
            entity.ReleaseDate.Should().Be(new DateTime(2024, 12, 25));
            entity.DurationMinutes.Should().Be(90);
            entity.Rating.Should().Be(6.5m);
            entity.Active.Should().BeFalse();
        }

        [Test]
        public void ConvertFilmUpdateDTOToEntity_Should_Preserve_Given_Id()
        {
            var dto = new FilmUpdateDTO
            {
                Title = "Title",
                Genre = Genre.Horror,
                ReleaseDate = new DateTime(2024, 1, 1),
                DurationMinutes = 100,
                Rating = 5.0m,
                Active = true
            };

            var entity = FilmMapperExtensions.ConvertFilmUpdateDTOToEntity("specific-id", dto);

            entity.Id.Should().Be("specific-id");
        }
    }
}
