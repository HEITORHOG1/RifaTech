using Microsoft.OpenApi.Models;
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
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetTicketsByRifa")
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
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetTicketById")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Obter detalhes de um ticket",
            Description = "Retorna os detalhes de um ticket específico pelo ID.",
            Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
        });

        app.MapPost("/rifa/{rifaId}/buy-ticket", async (string rifaId, TicketDTO ticket, ITicketService ticketService, ILogger<Program> logger) =>
        {
            try
            {
                var purchasedTicket = await ticketService.PurchaseTicketAsync(rifaId, ticket);
                logger.LogInformation($"Purchased ticket with ID {purchasedTicket} for rifa ID {rifaId} successfully");
                return Results.Ok(new { purchasedTicket });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error purchasing ticket for rifa ID {rifaId}");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
         .WithName("BuyTicket")
         .WithOpenApi(x => new OpenApiOperation(x)
         {
             Summary = "Comprar um ticket",
             Description = "Permite a um usuário comprar um ticket para uma rifa específica.",
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
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("UpdateTicket")
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
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("DeleteTicket")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Deletar um ticket",
            Description = "Remove um ticket existente.",
            Tags = new List<OpenApiTag> { new() { Name = "Tickets" } }
        });

        // Adicione mais endpoints conforme necessário
    }
}