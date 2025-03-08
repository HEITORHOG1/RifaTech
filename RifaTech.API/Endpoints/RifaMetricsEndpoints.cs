using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RifaTech.API.Context;
using RifaTech.API.Services;

namespace RifaTech.API.Endpoints
{
    public static class RifaMetricsEndpoints
    {
        public static void RegisterRifaMetricsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/rifas/metrics", async (
                AppDbContext context,
                ICacheService cacheService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var metrics = await cacheService.GetOrCreateAsync("rifas_metrics", async () =>
                    {
                        logger.LogInformation("Cache miss for rifa metrics, calculating from database");

                        var totalRifas = await context.Rifas
                            .Where(r => r.EhDeleted == false)
                            .CountAsync();

                        var activeRifas = await context.Rifas
                            .Where(r => r.EhDeleted == false && r.DrawDateTime > DateTime.UtcNow)
                            .CountAsync();

                        var totalTickets = await context.Tickets
                            .CountAsync();

                        var totalClientes = await context.Clientes
                            .CountAsync();

                        var upcomingDraws = await context.Rifas
                            .Where(r => r.EhDeleted == false &&
                                   r.DrawDateTime > DateTime.UtcNow &&
                                   r.DrawDateTime <= DateTime.UtcNow.AddDays(7))
                            .OrderBy(r => r.DrawDateTime)
                            .Select(r => new
                            {
                                r.Id,
                                r.Name,
                                r.DrawDateTime,
                                TimeRemaining = r.DrawDateTime.Subtract(DateTime.UtcNow).ToString(@"dd\d\ hh\h\ mm\m")
                            })
                            .Take(3)
                            .ToListAsync();

                        return new
                        {
                            TotalRifas = totalRifas,
                            ActiveRifas = activeRifas,
                            TotalTickets = totalTickets,
                            TotalClientes = totalClientes,
                            UpcomingDraws = upcomingDraws
                        };
                    }, TimeSpan.FromMinutes(5));

                    return Results.Ok(metrics);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving rifa metrics");
                    throw;
                }
            })
            .WithName("GetRifaMetrics")
            .AllowAnonymous() // Allow anonymous access
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter métricas de rifas",
                Description = "Retorna métricas e estatísticas sobre rifas, tickets e clientes.",
                Tags = new List<OpenApiTag> { new() { Name = "Rifas" } }
            });
        }
    }
}
