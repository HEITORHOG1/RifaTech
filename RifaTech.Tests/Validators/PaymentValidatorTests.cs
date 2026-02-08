using FluentAssertions;
using FluentValidation.TestHelper;
using RifaTech.API.Validators;
using RifaTech.DTOs.DTOs;

namespace RifaTech.Tests.Validators;

public class PaymentValidatorTests
{
    private readonly PaymentValidator _validator = new();

    private static PaymentDTO ValidDto() => new()
    {
        ClienteId = Guid.NewGuid(),
        TicketId = Guid.NewGuid(),
        Amount = 25.50m,
        Method = "PIX"
    };

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        var result = _validator.TestValidate(ValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_ClienteId_Is_Empty()
    {
        var dto = ValidDto();
        dto.ClienteId = Guid.Empty;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ClienteId)
            .WithErrorMessage("ClienteId é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_TicketId_Is_Empty()
    {
        var dto = ValidDto();
        dto.TicketId = Guid.Empty;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TicketId)
            .WithErrorMessage("TicketId é obrigatório.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_Fail_When_Amount_Is_Zero_Or_Negative(decimal amount)
    {
        var dto = ValidDto();
        dto.Amount = amount;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Valor do pagamento deve ser maior que zero.");
    }

    [Fact]
    public void Should_Fail_When_Method_Exceeds_MaxLength()
    {
        var dto = ValidDto();
        dto.Method = new string('X', 51);

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Method)
            .WithErrorMessage("Método de pagamento não pode exceder 50 caracteres.");
    }

    [Fact]
    public void Should_Pass_When_Method_Is_Null()
    {
        var dto = ValidDto();
        dto.Method = null;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Method);
    }
}
