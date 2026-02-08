using FluentValidation;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Validators;

public class RifaValidator : AbstractValidator<RifaDTO>
{
    public RifaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da rifa é obrigatório.")
            .MaximumLength(200).WithMessage("Nome não pode exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Descrição não pode exceder 2000 caracteres.");

        RuleFor(x => x.TicketPrice)
            .GreaterThan(0).WithMessage("Preço do ticket deve ser maior que zero.")
            .LessThanOrEqualTo(10_000m).WithMessage("Preço do ticket não pode exceder R$ 10.000,00.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de término é obrigatória.")
            .GreaterThan(x => x.StartDate).WithMessage("Data de término deve ser posterior à data de início.");

        RuleFor(x => x.DrawDateTime)
            .NotEmpty().WithMessage("Data do sorteio é obrigatória.")
            .GreaterThanOrEqualTo(x => x.EndDate).WithMessage("Data do sorteio deve ser igual ou posterior à data de término.");

        RuleFor(x => x.MinTickets)
            .GreaterThan(0).WithMessage("Mínimo de tickets deve ser maior que zero.");

        RuleFor(x => x.MaxTickets)
            .GreaterThan(0).WithMessage("Máximo de tickets deve ser maior que zero.")
            .GreaterThanOrEqualTo(x => x.MinTickets).WithMessage("Máximo de tickets deve ser maior ou igual ao mínimo.");

        RuleFor(x => x.PriceValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor do prêmio não pode ser negativo.");
    }
}
