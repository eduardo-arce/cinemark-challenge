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
    public class FilmUpdateValidatorTests
    {
        private FilmUpdateValidator _validator = null!;

        [SetUp]
        public void SetUp() => _validator = new FilmUpdateValidator();

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
            dto.Title = new string('B', 201);

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Título deve ter no máximo 200 caracteres");
        }

        [Test]
        public async Task Should_Fail_When_Genre_Is_Invalid()
        {
            var dto = ValidDto();
            dto.Genre = (Genre)0;

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
        public async Task Should_Fail_When_Rating_Is_Below_Zero()
        {
            var dto = ValidDto();
            dto.Rating = -0.1m;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Avaliação deve estar entre 0 e 10");
        }

        [Test]
        public async Task Should_Fail_When_Rating_Is_Above_Ten()
        {
            var dto = ValidDto();
            dto.Rating = 10.1m;

            var result = await _validator.ValidateAsync(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Avaliação deve estar entre 0 e 10");
        }
    }

}
