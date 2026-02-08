using Microsoft.OpenApi.Models;
using RifaTech.API.Exceptions;
using RifaTech.API.Validators;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Endpoints
{
    public static class RifaEndpoints
    {
        public static void RegisterRifaEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/rifas")
                .WithTags("Rifas");

            // =====================================================
            // PUBLIC ENDPOINTS (no authentication required)
            // =====================================================

            group.MapGet("/", async (IRifaService rifaService, ILogger<Program> logger) =>
            {
                var rifas = await rifaService.GetAllRifasAsync();
                logger.LogInformation("Retrieved {Count} rifas successfully", rifas.Count());
                return Results.Ok(rifas);
            })
            .WithName("GetRifas")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Listar todas as rifas",
                Description = "Retorna uma lista de todas as rifas disponíveis."
            });

            group.MapGet("/{id:guid}", async (Guid id, IRifaService rifaService, ILogger<Program> logger) =>
            {
                var rifa = await rifaService.GetRifaByIdAsync(id);
                if (rifa is null)
                {
                    logger.LogWarning("Rifa {RifaId} not found", id);
                    return Results.NotFound();
                }

                logger.LogInformation("Retrieved rifa {RifaId} successfully", id);
                return Results.Ok(rifa);
            })
            .WithName("GetRifaById")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter detalhes de uma rifa",
                Description = "Retorna os detalhes de uma rifa específica pelo ID."
            });

            group.MapGet("/destaque", async (IRifaService rifaService, ILogger<Program> logger) =>
            {
                var rifasDestaque = await rifaService.GetFeaturedRifasAsync();
                logger.LogInformation("Retrieved {Count} featured rifas", rifasDestaque.Count());
                return Results.Ok(rifasDestaque);
            })
            .WithName("GetFeaturedRifas")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Listar rifas em destaque",
                Description = "Retorna uma lista das rifas em destaque."
            });

            group.MapGet("/paginated", async (
                int pageNumber,
                int pageSize,
                string? searchTerm,
                DateTime? startDate,
                DateTime? endDate,
                IRifaService rifaService,
                ILogger<Program> logger) =>
            {
                var (rifas, totalCount) = await rifaService.GetRifasPaginatedAsync(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    startDate,
                    endDate);

                logger.LogInformation("Retrieved {Count}/{Total} paginated rifas (page {Page})",
                    rifas.Count(), totalCount, pageNumber);

                return Results.Ok(new { Items = rifas, TotalCount = totalCount });
            })
            .WithName("GetRifasPaginated")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Listar rifas com paginação",
                Description = "Retorna uma lista de rifas com paginação e opções de filtro."
            });

            // =====================================================
            // ADMIN ENDPOINTS (require Admin role)
            // =====================================================

            group.MapPost("/", async (RifaDTO rifa, IRifaService rifaService, ILogger<Program> logger) =>
            {
                rifa.EhDeleted = false;
                var createdRifa = await rifaService.CreateRifaAsync(rifa);
                logger.LogInformation("Created rifa {RifaId} with name '{RifaName}'", createdRifa.Id, createdRifa.Name);
                return Results.Created($"/rifas/{createdRifa.Id}", createdRifa);
            })
            .WithName("CreateRifa")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithValidation<RifaDTO>()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Criar uma nova rifa",
                Description = "Cria uma nova rifa com os dados fornecidos."
            });

            group.MapPut("/{id:guid}", async (Guid id, RifaDTO updatedRifa, IRifaService rifaService, ILogger<Program> logger) =>
            {
                var updated = await rifaService.UpdateRifaAsync(id, updatedRifa);
                if (updated is null)
                {
                    logger.LogWarning("Rifa {RifaId} not found for update", id);
                    return Results.NotFound(new ApiResponse<RifaDTO>
                    {
                        Data = null,
                        Success = false,
                        Message = "Rifa não encontrada."
                    });
                }

                logger.LogInformation("Updated rifa {RifaId} successfully", id);
                return Results.Ok(new ApiResponse<RifaDTO>
                {
                    Data = null,
                    Success = true,
                    Message = "Rifa atualizada com sucesso."
                });
            })
            .WithName("UpdateRifa")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithValidation<RifaDTO>()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Atualizar uma rifa",
                Description = "Atualiza os dados de uma rifa existente."
            });

            group.MapPatch("/{id:guid}/mark-as-deleted", async (Guid id, IRifaService rifaService, ILogger<Program> logger) =>
            {
                await rifaService.MarkRifaAsDeletedAsync(id);
                logger.LogInformation("Marked rifa {RifaId} as deleted", id);
                return Results.Ok();
            })
            .WithName("DeleteRifa")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Deletar uma rifa",
                Description = "Marca uma rifa existente como deletada, em vez de remover fisicamente do banco de dados."
            });
        }
    }
}