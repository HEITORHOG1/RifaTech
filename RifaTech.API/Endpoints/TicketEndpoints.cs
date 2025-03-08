using Microsoft.OpenApi.Models;
using RifaTech.API.Services;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

public static class TicketEndpoints
{
    public static void RegisterTicketEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rifa/{rifaId}/tickets", async (Guid rifaId, ITicketService ticketService, ILogger<Program> logger) =>
        {
            try
            {
                var tickets = await ticketService.GetTicketsByRifaAsync(rifaId);
                logger.LogInformation($"Retrieved tickets for rifa ID {rifaId} successfully");
                return Results.Ok(tickets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving tickets for rifa ID {rifaId}");
                throw;
            }
        })
         .WithName("GetTicketsByRifa")
         .AllowAnonymous() // Allow anonymous access
         .WithOpenApi(x => new OpenApiOperation(x)
         {
             Summary = "Listar todos os tickets de uma rifa",
             Description = "Retorna todos os tickets associados a uma rifa específica.",
             Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
         });

        app.MapGet("/ticket/{id}", async (string id, ITicketService ticketService, ILogger<Program> logger) =>
        {
            try
            {
                var ticket = await ticketService.GetTicketByIdAsync(id);
                if (ticket != null)
                {
                    logger.LogInformation($"Retrieved ticket with ID {id} successfully");
                    return Results.Ok(ticket);
                }
                else
                {
                    logger.LogWarning($"Ticket with ID {id} not found");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving ticket with ID {id}");
                throw;
            }
        })
         .WithName("GetTicketById")
         .AllowAnonymous() // Allow anonymous access
         .WithOpenApi(x => new OpenApiOperation(x)
         {
             Summary = "Obter detalhes de um ticket",
             Description = "Retorna os detalhes de um ticket específico pelo ID.",
             Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
         });

        // Traditional purchase - requires authentication
        app.MapPost("/rifa/{rifaId}/buy-ticket", async (string rifaId, TicketDTO ticket, ITicketService ticketService, ILogger<Program> logger) =>
        {
            try
            {
                var purchasedTicket = await ticketService.PurchaseTicketAsync(rifaId, ticket);
                logger.LogInformation($"Purchased ticket for rifa ID {rifaId} successfully");
                return Results.Ok(new { purchasedTicket });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error purchasing ticket for rifa ID {rifaId}");
                throw;
            }
        })
        .WithName("BuyTicket")
        .RequireAuthorization(policy => policy.RequireAuthenticatedUser()) // Require authentication
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Comprar um ticket",
            Description = "Permite a um usuário autenticado comprar um ticket para uma rifa específica.",
            Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
        });

        app.MapPut("/ticket/{id}", async (string id, TicketDTO updatedTicket, ITicketService ticketService, ILogger<Program> logger) =>
        {
            try
            {
                var updated = await ticketService.UpdateTicketAsync(id, updatedTicket);
                if (updated != null)
                {
                    logger.LogInformation($"Updated ticket with ID {id} successfully");
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning($"Ticket with ID {id} not found for update");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating ticket with ID {id}");
                throw;
            }
        })
         .WithName("UpdateTicket")
         .RequireAuthorization(policy => policy.RequireRole("Admin")) // Admin only
         .WithOpenApi(x => new OpenApiOperation(x)
         {
             Summary = "Atualizar um ticket",
             Description = "Atualiza informações de um ticket específico.",
             Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
         });

        app.MapDelete("/ticket/{id}", async (string id, ITicketService ticketService, ILogger<Program> logger) =>
        {
            try
            {
                await ticketService.CancelTicketAsync(id);
                logger.LogInformation($"Cancelled ticket with ID {id} successfully");
                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error cancelling ticket with ID {id}");
                throw;
            }
        })
        .WithName("DeleteTicket")
        .RequireAuthorization(policy => policy.RequireRole("Admin")) // Admin only
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Deletar um ticket",
            Description = "Remove um ticket existente.",
            Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
        });
    

    // New endpoint for quick purchase without authentication
    app.MapPost("/rifa/{rifaId}/compra-rapida", async (
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
        });
    }
}
