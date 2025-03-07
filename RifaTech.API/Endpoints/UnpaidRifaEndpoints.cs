using Microsoft.OpenApi.Models;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Endpoints
{
    public static class UnpaidRifaEndpoints
    {
        public static void RegisterUnpaidRifaEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/unpaidrifas", async (UnpaidRifaDTO unpaidRifaDto, IUnpaidRifaService unpaidRifaService, ILogger<Program> logger) =>
            {
                try
                {
                    var createdUnpaidRifa = await unpaidRifaService.CreateUnpaidRifaAsync(unpaidRifaDto);
                    logger.LogInformation($"Created unpaid rifa with ID {createdUnpaidRifa.RifaId} successfully");
                    return Results.Created($"/unpaidrifas/{createdUnpaidRifa.ClienteId}", createdUnpaidRifa);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating unpaid rifa");
                    throw;
                }
            })
            .WithName("CreateUnpaidRifa")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Criar uma nova rifa não paga",
                Description = "Cria uma nova rifa não paga.",
                Tags = new List<OpenApiTag> { new() { Name = "UnpaidRifas" } }
            });

            app.MapGet("/unpaidrifas", async (IUnpaidRifaService unpaidRifaService, ILogger<Program> logger) =>
            {
                try
                {
                    var unpaidRifas = await unpaidRifaService.GetAllUnpaidRifasAsync();
                    logger.LogInformation($"Retrieved {unpaidRifas.Count()} unpaid rifas successfully");
                    return Results.Ok(unpaidRifas);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving unpaid rifas");
                    throw;
                }
            })
            .WithName("GetAllUnpaidRifas")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter todas as rifas não pagas",
                Description = "Retorna uma lista de todas as rifas não pagas.",
                Tags = new List<OpenApiTag> { new() { Name = "UnpaidRifas" } }
            });
        }
    }
}