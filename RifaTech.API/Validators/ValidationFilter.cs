using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RifaTech.API.Validators;

/// <summary>
/// Endpoint filter que aplica FluentValidation automaticamente
/// em endpoints de Minimal API que recebem um DTO no body.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is null)
            return await next(context);

        // Procura o argumento do tipo T nos parâmetros do endpoint
        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (argument is null)
            return await next(context);

        var validationResult = await validator.ValidateAsync(argument);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors,
                title: "Erro de validação",
                detail: "Um ou mais campos possuem valores inválidos.");
        }

        return await next(context);
    }
}

/// <summary>
/// Extension methods para facilitar uso do ValidationFilter nos endpoints.
/// </summary>
public static class ValidationFilterExtensions
{
    /// <summary>
    /// Adiciona validação automática via FluentValidation ao endpoint.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}
