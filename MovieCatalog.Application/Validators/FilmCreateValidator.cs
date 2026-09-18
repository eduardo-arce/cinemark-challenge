using FluentValidation;
using MovieCatalog.Domain.DTO.Film;

namespace MovieCatalog.Application.Validators;

public class FilmCreateValidator : AbstractValidator<FilmCreateDTO>
{
    public FilmCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Gênero inválido");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Data de lançamento é obrigatória");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duração deve ser maior que 0");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).WithMessage("Avaliação deve estar entre 0 e 10");
    }
}
