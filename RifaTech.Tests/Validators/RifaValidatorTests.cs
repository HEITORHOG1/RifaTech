using FluentAssertions;
using FluentValidation.TestHelper;
using RifaTech.API.Validators;
using RifaTech.DTOs.DTOs;

namespace RifaTech.Tests.Validators;

public class RifaValidatorTests
{
    private readonly RifaValidator _validator = new();

    private static RifaDTO ValidDto() => new()
    {
        Name = "Rifa de Teste",
        Description = "Uma rifa para teste unitário",
        TicketPrice = 10.00m,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(7),
        DrawDateTime = DateTime.UtcNow.AddDays(8),
        MinTickets = 10,
        MaxTickets = 100,
        PriceValue = 500.00m
    };

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        var result = _validator.TestValidate(ValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var dto = ValidDto();
        dto.Name = "";

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome da rifa é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_TicketPrice_Is_Zero_Or_Negative()
    {
        var dto = ValidDto();
        dto.TicketPrice = 0;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TicketPrice)
            .WithErrorMessage("Preço do ticket deve ser maior que zero.");
    }

    [Fact]
    public void Should_Fail_When_TicketPrice_Exceeds_Limit()
    {
        var dto = ValidDto();
        dto.TicketPrice = 10_001m;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TicketPrice)
            .WithErrorMessage("Preço do ticket não pode exceder R$ 10.000,00.");
    }

    [Fact]
    public void Should_Fail_When_EndDate_Is_Before_StartDate()
    {
        var dto = ValidDto();
        dto.StartDate = DateTime.UtcNow.AddDays(5);
        dto.EndDate = DateTime.UtcNow;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("Data de término deve ser posterior à data de início.");
    }

    [Fact]
    public void Should_Fail_When_DrawDateTime_Is_Before_EndDate()
    {
        var dto = ValidDto();
        dto.EndDate = DateTime.UtcNow.AddDays(10);
        dto.DrawDateTime = DateTime.UtcNow.AddDays(5);

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DrawDateTime)
            .WithErrorMessage("Data do sorteio deve ser igual ou posterior à data de término.");
    }

    [Fact]
    public void Should_Fail_When_MaxTickets_Is_Less_Than_MinTickets()
    {
        var dto = ValidDto();
        dto.MinTickets = 50;
        dto.MaxTickets = 10;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MaxTickets)
            .WithErrorMessage("Máximo de tickets deve ser maior ou igual ao mínimo.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_Fail_When_MinTickets_Is_Zero_Or_Negative(int min)
    {
        var dto = ValidDto();
        dto.MinTickets = min;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MinTickets);
    }

    [Fact]
    public void Should_Fail_When_PriceValue_Is_Negative()
    {
        var dto = ValidDto();
        dto.PriceValue = -100m;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PriceValue)
            .WithErrorMessage("Valor do prêmio não pode ser negativo.");
    }
}
