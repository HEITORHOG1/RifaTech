using Microsoft.OpenApi.Models;
using RifaTech.API.Services;
using RifaTech.API.Validators;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Endpoints
{
    public static class CompraRapidaEndpoints
    {
        public static void RegisterCompraRapidaEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/compra-rapida/{rifaId}", async (
                string rifaId,
                CompraRapidaDTO compra,
                ICompraRapidaService compraRapidaService,
                ILogger<Program> logger) =>
            {
                var response = await compraRapidaService.ProcessarCompraRapidaAsync(rifaId, compra);
                logger.LogInformation("Compra rápida processada para rifa {RifaId}, {Quantidade} tickets",
                    rifaId, compra.Quantidade);
                return Results.Ok(response);
            })
            .WithName("CompraRapida")
            .AllowAnonymous()
            .RequireRateLimiting("PaymentEndpoints")
            .WithValidation<CompraRapidaDTO>()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Compra rápida de tickets",
                Description = "Permite comprar tickets para uma rifa sem necessidade de autenticação.",
                Tags = new List<OpenApiTag> { new() { Name = "Compras" } }
            });
        }
    }
}