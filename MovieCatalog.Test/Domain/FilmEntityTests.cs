using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Test.Domain
{

    [TestFixture]
    public class FilmEntityTests
    {
        private static FilmEntity CreateFilm() => new()
        {
            Id = "abc123",
            Title = "Test Film",
            Genre = Genre.Action,
            ReleaseDate = new DateTime(2024, 1, 1),
            DurationMinutes = 120,
            Rating = 8.0m,
            Active = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        [Test]
        public void MarkAsDeleted_Should_Set_IsDeleted_True()
        {
            var film = CreateFilm();

            film.MarkAsDeleted();

            film.IsDeleted.Should().BeTrue();
        }

        [Test]
        public void MarkAsDeleted_Should_Set_Active_False()
        {
            var film = CreateFilm();

            film.MarkAsDeleted();

            film.Active.Should().BeFalse();
        }

        [Test]
        public void MarkAsDeleted_Should_Update_UpdatedAt()
        {
            var film = CreateFilm();
            var before = film.UpdatedAt;

            film.MarkAsDeleted();

            film.UpdatedAt.Should().BeAfter(before);
        }

        [Test]
        public void SetUpdatedAt_Should_Update_Timestamp()
        {
            var film = CreateFilm();
            var before = film.UpdatedAt;

            film.SetUpdatedAt();

            film.UpdatedAt.Should().BeAfter(before);
        }

        [Test]
        public void New_Film_Should_Have_Default_Values()
        {
            var film = new FilmEntity();

            film.IsDeleted.Should().BeFalse();
            film.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            film.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

}
