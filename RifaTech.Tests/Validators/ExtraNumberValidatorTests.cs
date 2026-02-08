using FluentAssertions;
using FluentValidation.TestHelper;
using RifaTech.API.Validators;
using RifaTech.DTOs.DTOs;

namespace RifaTech.Tests.Validators;

public class ExtraNumberValidatorTests
{
    private readonly ExtraNumberValidator _validator = new();

    private static ExtraNumberDTO ValidDto() => new()
    {
        RifaId = Guid.NewGuid(),
        Number = 42,
        PrizeAmount = 100.00m
    };

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        var result = _validator.TestValidate(ValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_RifaId_Is_Empty()
    {
        var dto = ValidDto();
        dto.RifaId = Guid.Empty;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.RifaId)
            .WithErrorMessage("RifaId é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_Number_Is_Negative()
    {
        var dto = ValidDto();
        dto.Number = -1;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage("Número não pode ser negativo.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Should_Fail_When_PrizeAmount_Is_Zero_Or_Negative(decimal amount)
    {
        var dto = ValidDto();
        dto.PrizeAmount = amount;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PrizeAmount)
            .WithErrorMessage("Valor do prêmio deve ser maior que zero.");
    }

    [Fact]
    public void Should_Pass_When_Number_Is_Zero()
    {
        var dto = ValidDto();
        dto.Number = 0;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Number);
    }
}
