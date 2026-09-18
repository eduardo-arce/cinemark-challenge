using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using MovieCatalog.Application.Abstractions;
using MovieCatalog.Application.Service.Film;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.Enums;
using MovieCatalog.Domain.Events;
using MovieCatalog.Domain.Repositories;

namespace MovieCatalog.Test.Services
{
    [TestFixture]
    public class FilmUpdateServiceTests
    {
        private Mock<IFilmRepository> _repositoryMock = null!;
        private Mock<ICacheService> _cacheMock = null!;
        private Mock<IEventPublisher> _eventPublisherMock = null!;
        private Mock<IValidator<FilmUpdateDTO>> _validatorMock = null!;
        private Mock<ILogger<FilmUpdateService>> _loggerMock = null!;
        private FilmUpdateService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IFilmRepository>();
            _cacheMock = new Mock<ICacheService>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _validatorMock = new Mock<IValidator<FilmUpdateDTO>>();
            _loggerMock = new Mock<ILogger<FilmUpdateService>>();

            _service = new FilmUpdateService(
                _repositoryMock.Object,
                _cacheMock.Object,
                _eventPublisherMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }

        private static FilmUpdateDTO ValidDto() => new()
        {
            Title = "Updated Film",
            Synopsis = "Updated synopsis",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2024, 6, 15),
            DurationMinutes = 130,
            Rating = 7.5m,
            Active = true
        };

        private static FilmEntity ExistingFilm() => new()
        {
            Id = "abc123",
            Title = "Old Film",
            Genre = Genre.Action,
            ReleaseDate = new DateTime(2023, 1, 1),
            DurationMinutes = 100,
            Rating = 7.0m,
            Active = true
        };

        [Test]
        public async Task UpdateAsync_Should_Return_Ok_When_Valid()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123"))
                .ReturnsAsync(ExistingFilm());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), "abc123"))
                .ReturnsAsync(false);

            var result = await _service.UpdateAsync("abc123", dto);

            result.Status.Should().Be("OK");
            result.Content!.Title.Should().Be("Updated Film");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FilmEntity>()), Times.Once);
        }

        [Test]
        public void UpdateAsync_Should_Throw_ValidationException_When_Invalid()
        {
            var dto = ValidDto();
            dto.Title = "";
            var failures = new List<ValidationFailure> { new("Title", "Título é obrigatório") };
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(failures));

            var act = () => _service.UpdateAsync("abc123", dto);

            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public void UpdateAsync_Should_Throw_KeyNotFoundException_When_Film_Not_Found()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetByIdAsync("notfound"))
                .ReturnsAsync((FilmEntity?)null);

            var act = () => _service.UpdateAsync("notfound", dto);

            act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Test]
        public void UpdateAsync_Should_Throw_When_Duplicate_Title()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123"))
                .ReturnsAsync(ExistingFilm());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), "abc123"))
                .ReturnsAsync(true);

            var act = () => _service.UpdateAsync("abc123", dto);

            act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Já existe um filme com este título*");
        }

        [Test]
        public async Task UpdateAsync_Should_Invalidate_Cache()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123"))
                .ReturnsAsync(ExistingFilm());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), "abc123"))
                .ReturnsAsync(false);

            await _service.UpdateAsync("abc123", dto);

            _cacheMock.Verify(c => c.RemoveAsync("films:byid:abc123"), Times.Once);
            _cacheMock.Verify(c => c.RemoveByPrefixAsync("films:list"), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_Should_Publish_FilmUpdatedEvent()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.GetByIdAsync("abc123"))
                .ReturnsAsync(ExistingFilm());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), "abc123"))
                .ReturnsAsync(false);

            await _service.UpdateAsync("abc123", dto);

            _eventPublisherMock.Verify(e => e.PublishAsync(It.Is<FilmUpdatedEvent>(
                ev => ev.Title == "Updated Film" && ev.Genre == "Drama")), Times.Once);
        }
    }

}
