using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Application.Service.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Enums;
using MovieCatalog.Domain.Events;
using MovieCatalog.Domain.Repositories;

namespace MovieCatalog.Test.Services
{
    [TestFixture]
    public class FilmDeleteServiceTests
    {
        private Mock<IFilmRepository> _repositoryMock = null!;
        private Mock<ICacheService> _cacheMock = null!;
        private Mock<IEventPublisher> _eventPublisherMock = null!;
        private Mock<ILogger<FilmDeleteService>> _loggerMock = null!;
        private FilmDeleteService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IFilmRepository>();
            _cacheMock = new Mock<ICacheService>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<FilmDeleteService>>();

            _service = new FilmDeleteService(
                _repositoryMock.Object,
                _cacheMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);
        }

        private static FilmEntity ExistingFilm() => new()
        {
            Id = "abc123",
            Title = "Film To Delete",
            Genre = Genre.Horror,
            ReleaseDate = new DateTime(2023, 10, 31),
            DurationMinutes = 95,
            Rating = 6.5m,
            Active = true
        };

        [Test]
        public async Task DeleteAsync_Should_SoftDelete_And_Return_Ok()
        {
            var film = ExistingFilm();
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123")).ReturnsAsync(film);

            var result = await _service.DeleteAsync("abc123");

            result.Status.Should().Be("OK");
            result.Content!.IsDeleted.Should().BeTrue();
            result.Content.Active.Should().BeFalse();
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<FilmEntity>(
                f => f.IsDeleted && !f.Active)), Times.Once);
        }

        [Test]
        public void DeleteAsync_Should_Throw_KeyNotFoundException_When_Film_Not_Found()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("notfound"))
                .ReturnsAsync((FilmEntity?)null);

            var act = () => _service.DeleteAsync("notfound");

            act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*notfound*");
        }

        [Test]
        public async Task DeleteAsync_Should_Invalidate_Cache()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123")).ReturnsAsync(ExistingFilm());

            await _service.DeleteAsync("abc123");

            _cacheMock.Verify(c => c.RemoveAsync("films:byid:abc123"), Times.Once);
            _cacheMock.Verify(c => c.RemoveByPrefixAsync("films:list"), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_Should_Publish_FilmDeletedEvent()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123")).ReturnsAsync(ExistingFilm());

            await _service.DeleteAsync("abc123");

            _eventPublisherMock.Verify(e => e.PublishAsync(It.Is<FilmDeletedEvent>(
                ev => ev.FilmId == "abc123" && ev.Title == "Film To Delete")), Times.Once);
        }
    }

}
