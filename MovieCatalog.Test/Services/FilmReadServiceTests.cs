using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Application.Service.Film;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Enums;
using MovieCatalog.Domain.Repositories;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Test.Services
{
    [TestFixture]
    public class FilmReadServiceTests
    {
        private Mock<IFilmRepository> _repositoryMock = null!;
        private Mock<ICacheService> _cacheMock = null!;
        private FilmReadService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IFilmRepository>();
            _cacheMock = new Mock<ICacheService>();
            _service = new FilmReadService(_repositoryMock.Object, _cacheMock.Object);
        }

        private static FilmEntity SampleFilm() => new()
        {
            Id = "abc123",
            Title = "Test Film",
            Synopsis = "Synopsis",
            Genre = Genre.Action,
            ReleaseDate = new DateTime(2024, 1, 1),
            DurationMinutes = 120,
            Rating = 8.5m,
            Active = true
        };

        [Test]
        public async Task GetByIdAsync_Should_Return_Film_From_Cache_When_Available()
        {
            var film = SampleFilm();
            _cacheMock.Setup(c => c.GetAsync<FilmEntity>("films:byid:abc123"))
                .ReturnsAsync(film);

            var result = await _service.GetByIdAsync("abc123");

            result.Status.Should().Be("OK");
            result.Content!.Id.Should().Be("abc123");
            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Film_From_Repository_When_Not_Cached()
        {
            var film = SampleFilm();
            _cacheMock.Setup(c => c.GetAsync<FilmEntity>("films:byid:abc123"))
                .ReturnsAsync((FilmEntity?)null);
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123"))
                .ReturnsAsync(film);

            var result = await _service.GetByIdAsync("abc123");

            result.Status.Should().Be("OK");
            result.Content!.Title.Should().Be("Test Film");
            _cacheMock.Verify(c => c.SetAsync("films:byid:abc123", film, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Test]
        public void GetByIdAsync_Should_Throw_KeyNotFoundException_When_Film_Not_Found()
        {
            _cacheMock.Setup(c => c.GetAsync<FilmEntity>(It.IsAny<string>()))
                .ReturnsAsync((FilmEntity?)null);
            _repositoryMock.Setup(r => r.GetByIdAsync("notfound"))
                .ReturnsAsync((FilmEntity?)null);

            var act = () => _service.GetByIdAsync("notfound");

            act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*notfound*");
        }

        [Test]
        public async Task GetAllAsync_Should_Return_Paginated_From_Cache_When_Available()
        {
            var filterDto = new FilmFilterDTO { Page = 1, PageSize = 10 };
            var cached = new PaginatedResult<FilmEntity>
            {
                Page = 1,
                PageSize = 10,
                TotalItems = 1,
                Items = new List<FilmEntity> { SampleFilm() }
            };
            _cacheMock.Setup(c => c.GetAsync<PaginatedResult<FilmEntity>>(It.IsAny<string>()))
                .ReturnsAsync(cached);

            var result = await _service.GetAllAsync(filterDto);

            result.Status.Should().Be("OK");
            result.Content!.TotalItems.Should().Be(1);
            _repositoryMock.Verify(r => r.GetPagedAsync(
                It.IsAny<Expression<Func<FilmEntity, bool>>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetAllAsync_Should_Query_Repository_When_Not_Cached()
        {
            var filterDto = new FilmFilterDTO { Page = 1, PageSize = 10 };
            _cacheMock.Setup(c => c.GetAsync<PaginatedResult<FilmEntity>>(It.IsAny<string>()))
                .ReturnsAsync((PaginatedResult<FilmEntity>?)null);

            var films = new List<FilmEntity> { SampleFilm() };
            _repositoryMock.Setup(r => r.GetPagedAsync(
                It.IsAny<Expression<Func<FilmEntity, bool>>>(), 1, 10))
                .ReturnsAsync((films as IReadOnlyList<FilmEntity>, 1L));

            var result = await _service.GetAllAsync(filterDto);

            result.Status.Should().Be("OK");
            result.Content!.TotalItems.Should().Be(1);
            result.Content.Page.Should().Be(1);
        }

        [Test]
        public async Task GetAllAsync_Should_Apply_Genre_Filter()
        {
            var filterDto = new FilmFilterDTO { Genre = Genre.Action, Page = 1, PageSize = 10 };
            _cacheMock.Setup(c => c.GetAsync<PaginatedResult<FilmEntity>>(It.IsAny<string>()))
                .ReturnsAsync((PaginatedResult<FilmEntity>?)null);
            _repositoryMock.Setup(r => r.GetPagedAsync(
                It.IsAny<Expression<Func<FilmEntity, bool>>>(), 1, 10))
                .ReturnsAsync((new List<FilmEntity>() as IReadOnlyList<FilmEntity>, 0L));

            var result = await _service.GetAllAsync(filterDto);

            result.Status.Should().Be("OK");
            _repositoryMock.Verify(r => r.GetPagedAsync(
                It.IsNotNull<Expression<Func<FilmEntity, bool>>>(), 1, 10), Times.Once);
        }
    }

}
