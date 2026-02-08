using FluentAssertions;
using FluentValidation.TestHelper;
using RifaTech.API.Validators;
using RifaTech.DTOs.DTOs;

namespace RifaTech.Tests.Validators;

public class ClienteValidatorTests
{
    private readonly ClienteValidator _validator = new();

    private static ClienteDTO ValidDto() => new()
    {
        Name = "Maria Souza",
        Email = "maria@email.com",
        PhoneNumber = "+5521988776655",
        CPF = "987.654.321-00"
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
            .WithErrorMessage("Nome é obrigatório.");
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var dto = ValidDto();
        dto.Name = new string('X', 101);

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Nome não pode exceder 100 caracteres.");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@.com")]
    public void Should_Fail_When_Email_Is_Invalid(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email em formato inválido.");
    }

    [Fact]
    public void Should_Pass_When_Email_Is_NullOrEmpty()
    {
        var dto = ValidDto();
        dto.Email = null;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("12345678900")]
    public void Should_Pass_When_CPF_Is_Valid(string cpf)
    {
        var dto = ValidDto();
        dto.CPF = cpf;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CPF);
    }

    [Fact]
    public void Should_Pass_When_CPF_Is_Null()
    {
        var dto = ValidDto();
        dto.CPF = null;

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CPF);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("000")]
    public void Should_Fail_When_CPF_Has_Invalid_Format(string cpf)
    {
        var dto = ValidDto();
        dto.CPF = cpf;

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CPF);
    }
}
