using FluentValidation;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Validators;

public class ExtraNumberValidator : AbstractValidator<ExtraNumberDTO>
{
    public ExtraNumberValidator()
    {
        RuleFor(x => x.RifaId)
            .NotEmpty().WithMessage("RifaId é obrigatório.");

        RuleFor(x => x.Number)
            .GreaterThanOrEqualTo(0).WithMessage("Número não pode ser negativo.");

        RuleFor(x => x.PrizeAmount)
            .GreaterThan(0).WithMessage("Valor do prêmio deve ser maior que zero.");
    }
}
