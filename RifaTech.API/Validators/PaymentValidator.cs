using FluentValidation;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Validators;

public class PaymentValidator : AbstractValidator<PaymentDTO>
{
    public PaymentValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("ClienteId é obrigatório.");

        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId é obrigatório.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor do pagamento deve ser maior que zero.");

        RuleFor(x => x.Method)
            .MaximumLength(50).WithMessage("Método de pagamento não pode exceder 50 caracteres.");
    }
}
