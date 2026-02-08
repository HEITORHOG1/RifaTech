using FluentValidation;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Validators;

public class CompraRapidaValidator : AbstractValidator<CompraRapidaDTO>
{
    public CompraRapidaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(60).WithMessage("Nome não pode exceder 60 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email em formato inválido.")
            .MaximumLength(100).WithMessage("Email não pode exceder 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone não pode exceder 20 caracteres.")
            .Matches(@"^\+?\d[\d\s\-()]{7,18}$").WithMessage("Telefone em formato inválido.");

        RuleFor(x => x.CPF)
            .MaximumLength(14).WithMessage("CPF não pode exceder 14 caracteres.")
            .Matches(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CPF))
                .WithMessage("CPF em formato inválido. Use 000.000.000-00 ou 00000000000.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.")
            .LessThanOrEqualTo(100).WithMessage("Quantidade não pode exceder 100 tickets.");
    }
}
