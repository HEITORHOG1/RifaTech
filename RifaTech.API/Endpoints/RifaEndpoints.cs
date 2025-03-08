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
            // Public endpoints - no authentication required

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
                    throw;
                }
            })
            .WithName("GetRifas")
            .AllowAnonymous() // Explicitly allow anonymous access
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
                    throw;
                }
            })
            .WithName("GetRifaById")
            .AllowAnonymous() // Explicitly allow anonymous access
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter detalhes de uma rifa",
                Description = "Retorna os detalhes de uma rifa específica pelo ID.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            app.MapGet("/rifas/destaque", async (IRifaService rifaService, ILogger<Program> logger) =>
            {
                try
                {
                    // Assume we have a new method in the service to get featured rifas
                    var rifasDestaque = await rifaService.GetFeaturedRifasAsync();
                    logger.LogInformation("Retrieved featured rifas successfully");
                    return Results.Ok(rifasDestaque);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving featured rifas");
                    throw;
                }
            })
            .WithName("GetFeaturedRifas")
            .AllowAnonymous() // Explicitly allow anonymous access
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Listar rifas em destaque",
                Description = "Retorna uma lista das rifas em destaque.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            // Admin-only endpoints - require admin authentication

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
                    throw;
                }
            })
            .WithName("CreateRifa")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Require Admin role
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
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("UpdateRifa")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Require Admin role
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
                    await rifaService.MarkRifaAsDeletedAsync(id);
                    logger.LogInformation($"Marked rifa with ID {id} as deleted successfully");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error marking rifa with ID {id} as deleted");
                    throw;
                }
            })
            .WithName("DeleteRifa")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Require Admin role
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Deletar uma rifa",
                Description = "Marca uma rifa existente como deletada, em vez de remover fisicamente do banco de dados.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });

            // Endpoint for paginated rifas (public)
            app.MapGet("/rifas/paginated", async (
                int pageNumber,
                int pageSize,
                string? searchTerm,
                DateTime? startDate,
                DateTime? endDate,
                IRifaService rifaService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var (rifas, totalCount) = await rifaService.GetRifasPaginatedAsync(
                        pageNumber,
                        pageSize,
                        searchTerm,
                        startDate,
                        endDate);

                    return Results.Ok(new { Items = rifas, TotalCount = totalCount });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving paginated rifas");
                    throw;
                }
            })
            .WithName("GetRifasPaginated")
            .AllowAnonymous() // Explicitly allow anonymous access
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Listar rifas com paginação",
                Description = "Retorna uma lista de rifas com paginação e opções de filtro.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });
        }
    }
}