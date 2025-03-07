using Microsoft.OpenApi.Models;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

public static class ExtraNumberEndpoints
{
    public static void RegisterExtraNumberEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rifas/{rifaId}/extra-numbers", async (string rifaId, IExtraNumberService extraNumberService, ILogger<Program> logger) =>
        {
            try
            {
                var extraNumbers = await extraNumberService.GetExtraNumbersByRifaAsync(rifaId);
                logger.LogInformation($"Retrieved extra numbers for rifa ID {rifaId} successfully");
                return Results.Ok(extraNumbers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving extra numbers for rifa ID {rifaId}");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetExtraNumbersByRifa")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Listar números extras de uma rifa",
            Description = "Retorna todos os números extras associados a uma rifa específica.",
            Tags = new List<OpenApiTag> { new() { Name = "Números Extras" } }
        });

        app.MapPost("/rifas/{rifaId}/extra-number", async (string rifaId, ExtraNumberDTO extraNumber, IExtraNumberService extraNumberService, ILogger<Program> logger) =>
        {
            try
            {
                var createdExtraNumber = await extraNumberService.AddExtraNumberAsync(rifaId, extraNumber);
                logger.LogInformation($"Created extra number for rifa ID {rifaId} successfully");
                return Results.Created($"/extra-number/{createdExtraNumber.Id}", createdExtraNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating extra number for rifa ID {rifaId}");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("AddExtraNumber")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Adicionar números extras a uma rifa",
            Description = "Adiciona um ou mais números extras a uma rifa específica.",
            Tags = new List<OpenApiTag> { new() { Name = "Números Extras" } }
        });
    }
}