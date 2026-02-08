using FluentValidation;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Validators;

public class ClienteValidator : AbstractValidator<ClienteDTO>
{
    public ClienteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email em formato inválido.")
            .MaximumLength(100).WithMessage("Email não pode exceder 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefone não pode exceder 20 caracteres.")
            .Matches(@"^\+?\d[\d\s\-()]{7,18}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Telefone em formato inválido.");

        RuleFor(x => x.CPF)
            .MaximumLength(14).WithMessage("CPF não pode exceder 14 caracteres.")
            .Matches(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CPF))
                .WithMessage("CPF em formato inválido.");
    }
}
