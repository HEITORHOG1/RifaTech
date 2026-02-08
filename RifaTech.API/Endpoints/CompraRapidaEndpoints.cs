using Microsoft.OpenApi.Models;
using RifaTech.API.Services;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Endpoints
{
    public static class CompraRapidaEndpoints
    {
        public static void RegisterCompraRapidaEndpoints(this IEndpointRouteBuilder app)
        {
            // Endpoint for quick purchase without authentication
            app.MapPost("/compra-rapida/{rifaId}", async (
                string rifaId,
                CompraRapidaDTO compra,
                ICompraRapidaService compraRapidaService,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Use the dedicated service to handle the purchase
                    var response = await compraRapidaService.ProcessarCompraRapidaAsync(rifaId, compra);
                    logger.LogInformation($"Compra rápida processada com sucesso para rifa ID {rifaId}");

                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex, $"Rifa não encontrada: {rifaId}");
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning(ex, $"Operação inválida para rifa ID {rifaId}: {ex.Message}");
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Erro durante compra rápida para rifa ID {rifaId}");
                    return Results.Problem($"Erro ao processar compra: {ex.Message}");
                }
            })
            .WithName("CompraRapida")
            .AllowAnonymous() // Allow anonymous access
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Compra rápida de tickets",
                Description = "Permite comprar tickets para uma rifa sem necessidade de autenticação.",
                Tags = new List<OpenApiTag> { new() { Name = "Compras" } }
            });
        }
    }
}