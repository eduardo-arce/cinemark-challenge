using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MovieCatalog.Application.Validators;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Test.Validators
{
    [TestFixture]
    public class FilmCreateValidatorTests
    {
        private FilmCreateValidator _validator = null!;

        [SetUp]
        public void SetUp() => _validator = new FilmCreateValidator();

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
        public async Task Should_Pass_When_All_Fields_Are_Valid()
        {
            var result = await _validator.ValidateAsync(ValidDto());
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task Should_Fail_When_Title_Is_Empty()
        {
            var dto = ValidDto();
            dto.Title = "";

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Título é obrigatório");
        }

        [Test]
        public async Task Should_Fail_When_Title_Exceeds_200_Characters()
        {
            var dto = ValidDto();
            dto.Title = new string('A', 201);

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Título deve ter no máximo 200 caracteres");
        }

        [Test]
        public async Task Should_Fail_When_Genre_Is_Invalid()
        {
            var dto = ValidDto();
            dto.Genre = (Genre)999;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Gênero inválido");
        }

        [Test]
        public async Task Should_Fail_When_DurationMinutes_Is_Zero()
        {
            var dto = ValidDto();
            dto.DurationMinutes = 0;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Duração deve ser maior que 0");
        }

        [Test]
        public async Task Should_Fail_When_DurationMinutes_Is_Negative()
        {
            var dto = ValidDto();
            dto.DurationMinutes = -5;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Duração deve ser maior que 0");
        }

        [Test]
        public async Task Should_Fail_When_Rating_Is_Below_Zero()
        {
            var dto = ValidDto();
            dto.Rating = -1m;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Avaliação deve estar entre 0 e 10");
        }

        [Test]
        public async Task Should_Fail_When_Rating_Is_Above_Ten()
        {
            var dto = ValidDto();
            dto.Rating = 11m;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Avaliação deve estar entre 0 e 10");
        }

        [Test]
        public async Task Should_Pass_When_Rating_Is_Zero()
        {
            var dto = ValidDto();
            dto.Rating = 0m;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task Should_Pass_When_Rating_Is_Ten()
        {
            var dto = ValidDto();
            dto.Rating = 10m;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task Should_Return_Multiple_Errors_When_Multiple_Fields_Invalid()
        {
            var dto = new FilmCreateDTO
            {
                Title = "",
                Genre = (Genre)999,
                DurationMinutes = 0,
                Rating = 15m,
                ReleaseDate = default
            };

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().BeGreaterThanOrEqualTo(3);
        }
    }

}
