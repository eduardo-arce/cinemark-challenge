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
    public class FilmCreateServiceTests
    {
        private Mock<IFilmRepository> _repositoryMock = null!;
        private Mock<ICacheService> _cacheMock = null!;
        private Mock<IEventPublisher> _eventPublisherMock = null!;
        private Mock<IValidator<FilmCreateDTO>> _validatorMock = null!;
        private Mock<ILogger<FilmCreateService>> _loggerMock = null!;
        private FilmCreateService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IFilmRepository>();
            _cacheMock = new Mock<ICacheService>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _validatorMock = new Mock<IValidator<FilmCreateDTO>>();
            _loggerMock = new Mock<ILogger<FilmCreateService>>();

            _service = new FilmCreateService(
                _repositoryMock.Object,
                _cacheMock.Object,
                _eventPublisherMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }

        private static FilmCreateDTO ValidDto() => new()
        {
            Title = "Test Film",
            Synopsis = "A great film",
            Genre = Genre.Action,
            ReleaseDate = new DateTime(2024, 1, 1),
            DurationMinutes = 120,
            Rating = 8.5m,
            Active = true
        };

        [Test]
        public async Task CreateAsync_Should_Return_Created_When_Valid()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), null))
                .ReturnsAsync(false);

            var result = await _service.CreateAsync(dto);

            result.Status.Should().Be("CREATED");
            result.IsSuccess.Should().BeTrue();
            result.Content.Should().NotBeNull();
            result.Content!.Title.Should().Be("Test Film");
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FilmEntity>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveByPrefixAsync("films:list"), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<FilmCreatedEvent>()), Times.Once);
        }

        [Test]
        public void CreateAsync_Should_Throw_ValidationException_When_Validation_Fails()
        {
            var dto = ValidDto();
            dto.Title = "";
            var failures = new List<ValidationFailure>
        {
            new("Title", "Título é obrigatório")
        };
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(failures));

            var act = () => _service.CreateAsync(dto);

            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public void CreateAsync_Should_Throw_When_Title_Already_Exists()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), null))
                .ReturnsAsync(true);

            var act = () => _service.CreateAsync(dto);

            act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Já existe um filme com este título*");
        }

        [Test]
        public async Task CreateAsync_Should_Invalidate_List_Cache()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), null))
                .ReturnsAsync(false);

            await _service.CreateAsync(dto);

            _cacheMock.Verify(c => c.RemoveByPrefixAsync("films:list"), Times.Once);
        }

        [Test]
        public async Task CreateAsync_Should_Publish_FilmCreatedEvent()
        {
            var dto = ValidDto();
            _validatorMock.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock.Setup(r => r.ExistsByTitleAsync(dto.Title.Trim(), null))
                .ReturnsAsync(false);

            await _service.CreateAsync(dto);

            _eventPublisherMock.Verify(e => e.PublishAsync(It.Is<FilmCreatedEvent>(
                ev => ev.Title == "Test Film" && ev.Genre == "Action")), Times.Once);
        }
    }
}
