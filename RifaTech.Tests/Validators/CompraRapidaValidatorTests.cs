using FluentAssertions;
using FluentValidation.TestHelper;
using RifaTech.API.Validators;
using RifaTech.DTOs.DTOs;

namespace RifaTech.Tests.Validators;

public class CompraRapidaValidatorTests
{
    private readonly CompraRapidaValidator _validator = new();

    private static CompraRapidaDTO ValidDto() => new()
    {
        Name = "João Silva",
        Email = "joao@email.com",
        PhoneNumber = "+5511999887766",
        CPF = "123.456.789-00",
        Quantidade = 5
    };

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        var result = _validator.TestValidate(ValidDto());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ===== Name =====
    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var dto = ValidDto();
        dto.Name = "";

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var dto = ValidDto();
        dto.Name = new string('A', 61);

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome não pode exceder 60 caracteres.");
    }

    // ===== Email =====
    [Fact]
    public void Should_Fail_When_Email_Is_Empty()
    {
        var dto = ValidDto();
        dto.Email = "";

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@no-local.com")]
    [InlineData("missing-domain@")]
    public void Should_Fail_When_Email_Format_Is_Invalid(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email em formato inválido.");
    }

    // ===== PhoneNumber =====
    [Fact]
    public void Should_Fail_When_PhoneNumber_Is_Empty()
    {
        var dto = ValidDto();
        dto.PhoneNumber = "";

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // ===== CPF =====
    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("12345678900")]
    public void Should_Pass_When_CPF_Has_Valid_Format(string cpf)
    {
        var dto = ValidDto();
        dto.CPF = cpf;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CPF);
    }

    [Fact]
    public void Should_Pass_When_CPF_Is_NullOrEmpty()
    {
        var dto = ValidDto();
        dto.CPF = null!;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CPF);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12345")]
    [InlineData("123-456-789-00")]
    public void Should_Fail_When_CPF_Has_Invalid_Format(string cpf)
    {
        var dto = ValidDto();
        dto.CPF = cpf;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CPF);
    }

    // ===== Quantidade =====
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_Quantidade_Is_Zero_Or_Negative(int qtd)
    {
        var dto = ValidDto();
        dto.Quantidade = qtd;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Quantidade);
    }

    [Fact]
    public void Should_Fail_When_Quantidade_Exceeds_100()
    {
        var dto = ValidDto();
        dto.Quantidade = 101;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Quantidade)
            .WithErrorMessage("Quantidade não pode exceder 100 tickets.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_Pass_When_Quantidade_Is_In_Valid_Range(int qtd)
    {
        var dto = ValidDto();
        dto.Quantidade = qtd;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Quantidade);
    }
}
