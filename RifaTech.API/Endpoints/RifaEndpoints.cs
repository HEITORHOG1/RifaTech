using Microsoft.OpenApi.Models;
using RifaTech.API.Exceptions;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Endpoints
{
    public static class RifaEndpoints
    {
        public static void RegisterRifaEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/rifas", async (IRifaService rifaService, ILogger<Program> logger) =>
            {
                try
                {
                    var rifas = await rifaService.GetAllRifasAsync();
                    logger.LogInformation("Retrieved all rifas successfully");
                    return Results.Ok(rifas);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving rifas");
                    throw; // O middleware de tratamento de exceções capturará isso
                }
            })
            .WithName("GetRifas")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Listar todas as rifas",
                Description = "Retorna uma lista de todas as rifas disponíveis.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            app.MapGet("/rifas/{id}", async (Guid id, IRifaService rifaService, ILogger<Program> logger) =>
            {
                try
                {
                    var rifa = await rifaService.GetRifaByIdAsync(id);
                    if (rifa != null)
                    {
                        logger.LogInformation($"Retrieved rifa with ID {id} successfully");
                        return Results.Ok(rifa);
                    }
                    else
                    {
                        logger.LogWarning($"Rifa with ID {id} not found");
                        return Results.NotFound();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error retrieving rifa with ID {id}");
                    throw; // O middleware de tratamento de exceções capturará isso
                }
            })
            .WithName("GetRifaById")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter detalhes de uma rifa",
                Description = "Retorna os detalhes de uma rifa específica pelo ID.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            app.MapPost("/rifas", async (RifaDTO rifa, IRifaService rifaService, ILogger<Program> logger) =>
            {
                try
                {
                    rifa.EhDeleted = false;
                    var createdRifa = await rifaService.CreateRifaAsync(rifa);
                    logger.LogInformation($"Created rifa with ID {createdRifa.Id} successfully");
                    return Results.Created($"/rifa/{createdRifa.Id}", createdRifa);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating rifa");
                    throw; // O middleware de tratamento de exceções capturará isso
                }
            })
            .WithName("CreateRifa")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Criar uma nova rifa",
                Description = "Cria uma nova rifa com os dados fornecidos.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            app.MapPut("/rifas/{id}", async (Guid id, RifaDTO updatedRifa, IRifaService rifaService, ILogger<Program> logger) =>
            {
                try
                {
                    var updated = await rifaService.UpdateRifaAsync(id, updatedRifa);
                    if (updated != null)
                    {
                        logger.LogInformation($"Updated rifa with ID {id} successfully");
                        return Results.Ok(new ApiResponse<RifaDTO>
                        {
                            Data = null,
                            Success = true,
                            Message = "Rifa atualizada com sucesso."
                        });
                    }
                    else
                    {
                        logger.LogWarning($"Rifa with ID {id} not found for update");
                        return Results.NotFound(new ApiResponse<RifaDTO>
                        {
                            Data = null,
                            Success = false,
                            Message = "Rifa não encontrada."
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error updating rifa with ID {id}");
                    return Results.Problem(ex.Message); // Retorna um status de erro com a mensagem da exceção
                }
            })
            .WithName("UpdateRifa")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Atualizar uma rifa",
                Description = "Atualiza os dados de uma rifa existente.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            app.MapPatch("/rifas/{id}/mark-as-deleted", async (Guid id, IRifaService rifaService, ILogger<Program> logger) =>
            {
                try
                {
                    // Chama o método de serviço que marca a rifa como deletada, em vez de deletá-la
                    await rifaService.MarkRifaAsDeletedAsync(id);
                    logger.LogInformation($"Marked rifa with ID {id} as deleted successfully");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error marking rifa with ID {id} as deleted");
                    throw; // O middleware de tratamento de exceções capturará isso
                }
            })
            .WithName("DeleteRifa")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Deletar uma rifa",
                Description = "Marca uma rifa existente como deletada, em vez de remover fisicamente do banco de dados.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });
        }
    }
}