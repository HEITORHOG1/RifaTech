using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

public static class DrawEndpoints
{
    public static void RegisterDrawEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/draws", async (IDrawService drawService, ILogger<Program> logger) =>
        {
            try
            {
                var draws = await drawService.GetAllDrawsAsync();
                logger.LogInformation("Retrieved all draws successfully");
                return Results.Ok(draws);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving draws");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetAllDraws")
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Listar todos os sorteios",
            Description = "Retorna uma lista de todos os sorteios realizados ou programados.",
            Tags = new List<OpenApiTag> { new() { Name = "Sorteios" } }
        });

        app.MapGet("/draw/{id}", async (string id, IDrawService drawService, ILogger<Program> logger) =>
        {
            try
            {
                var draw = await drawService.GetDrawByIdAsync(id);
                if (draw != null)
                {
                    logger.LogInformation($"Retrieved draw with ID {id} successfully");
                    return Results.Ok(draw);
                }
                else
                {
                    logger.LogWarning($"Draw with ID {id} not found");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving draw with ID {id}");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetDrawById")
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Obter detalhes de um sorteio",
            Description = "Retorna os detalhes de um sorteio específico pelo ID.",
            Tags = new List<OpenApiTag> { new() { Name = "Sorteios" } }
        });

        app.MapPost("/rifa/{rifaId}/draw", async (string rifaId, DrawDTO draw, IDrawService drawService, ILogger<Program> logger) =>
        {
            try
            {
                var createdDraw = await drawService.CreateDrawAsync(rifaId, draw);
                logger.LogInformation($"Created draw for rifa ID {rifaId} successfully");
                return Results.Created($"/draw/{createdDraw.Id}", createdDraw);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating draw for rifa ID {rifaId}");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("CreateDraw")
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Criar um novo sorteio para uma rifa",
            Description = "Cria um novo sorteio para uma rifa específica.",
            Tags = new List<OpenApiTag> { new() { Name = "Sorteios" } }
        });
    }
}