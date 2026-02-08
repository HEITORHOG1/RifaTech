using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using RifaTech.API.Services;
using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Endpoints
{
    public static class TicketSearchEndpoints
    {
        public static void RegisterTicketSearchEndpoints(this IEndpointRouteBuilder app)
        {
            // Endpoint para pesquisa avançada de tickets
            app.MapGet("/admin/tickets/search", async (
                [FromQuery] Guid? rifaId,
                [FromQuery] Guid? clienteId,
                [FromQuery] int? ticketNumber,
                [FromQuery] string rifaName,
                [FromQuery] string clienteName,
                [FromQuery] string clienteEmail,
                [FromQuery] string clientePhone,
                [FromQuery] bool? validOnly,
                [FromQuery] DateTime? createdAfter,
                [FromQuery] DateTime? createdBefore,
                [FromQuery] string orderBy,
                [FromQuery] string orderDirection,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                ITicketSearchService ticketSearchService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var query = new TicketSearchQueryDTO
                    {
                        RifaId = rifaId,
                        ClienteId = clienteId,
                        TicketNumber = ticketNumber,
                        RifaName = rifaName,
                        ClienteName = clienteName,
                        ClienteEmail = clienteEmail,
                        ClientePhone = clientePhone,
                        ValidOnly = validOnly,
                        CreatedAfter = createdAfter,
                        CreatedBefore = createdBefore,
                        OrderBy = orderBy,
                        OrderDirection = orderDirection,
                        PageNumber = pageNumber <= 0 ? 1 : pageNumber,
                        PageSize = pageSize <= 0 ? 20 : pageSize
                    };

                    var result = await ticketSearchService.SearchTicketsAsync(query);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error searching tickets");
                    return Results.Problem("Error searching tickets");
                }
            })
            .WithName("SearchTickets")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Pesquisar tickets",
                Description = "Pesquisa avançada de tickets com filtros, ordenação e paginação.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para resumo de tickets por rifa
            app.MapGet("/admin/tickets/summary/{rifaId}", async (
                Guid rifaId,
                ITicketSearchService ticketSearchService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var summary = await ticketSearchService.GetTicketSummaryByRifaAsync(rifaId);
                    return Results.Ok(summary);
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error retrieving ticket summary for rifa {rifaId}");
                    return Results.Problem("Error retrieving ticket summary");
                }
            })
            .WithName("GetTicketSummary")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter resumo de tickets",
                Description = "Retorna um resumo dos tickets para uma rifa específica, incluindo estatísticas e tendências.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Corrigido: Endpoint para buscar tickets por números específicos
            // Mudamos para receber uma string e fazer o parse manualmente
            app.MapGet("/admin/tickets/by-numbers/{rifaId}", async (
                Guid rifaId,
                [FromQuery] string numbersParam,
                ITicketSearchService ticketSearchService,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Faz o parse da string para uma lista de inteiros
                    List<int> numbers = new List<int>();
                    if (!string.IsNullOrEmpty(numbersParam))
                    {
                        string[] parts = numbersParam.Split(',');
                        foreach (var part in parts)
                        {
                            if (int.TryParse(part.Trim(), out int number))
                            {
                                numbers.Add(number);
                            }
                        }
                    }

                    if (numbers.Count == 0)
                    {
                        return Results.BadRequest(new { message = "Deve fornecer pelo menos um número de ticket" });
                    }

                    var tickets = await ticketSearchService.GetTicketsByNumbersAsync(rifaId, numbers);
                    return Results.Ok(tickets);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error retrieving tickets by numbers for rifa {rifaId}");
                    return Results.Problem("Error retrieving tickets by numbers");
                }
            })
            .WithName("GetTicketsByNumbers")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Buscar tickets por números",
                Description = "Busca tickets por números específicos em uma rifa. Forneça os números separados por vírgula (ex: 1,2,3,4).",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para buscar tickets por cliente
            app.MapGet("/admin/tickets/by-cliente/{clienteId}", async (
                Guid clienteId,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                ITicketSearchService ticketSearchService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var tickets = await ticketSearchService.GetTicketsByClienteAsync(
                        clienteId,
                        pageNumber <= 0 ? 1 : pageNumber,
                        pageSize <= 0 ? 20 : pageSize);

                    return Results.Ok(tickets);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error retrieving tickets for cliente {clienteId}");
                    return Results.Problem("Error retrieving tickets by cliente");
                }
            })
            .WithName("GetTicketsByCliente")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Buscar tickets por cliente",
                Description = "Busca tickets comprados por um cliente específico.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });
        }
    }
}