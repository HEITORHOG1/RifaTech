using Microsoft.OpenApi.Models;
using RifaTech.API.Services;

namespace RifaTech.API.Endpoints
{
    public static class AdminDashboardEndpoints
    {
        public static void RegisterAdminDashboardEndpoints(this IEndpointRouteBuilder app)
        {
            // Endpoint para estatísticas gerais do dashboard
            app.MapGet("/admin/dashboard/stats", async (
                IAdminStatsService adminStatsService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var stats = await adminStatsService.GetDashboardStatsAsync();
                    return Results.Ok(stats);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving admin dashboard stats");
                    return Results.Problem("Error retrieving dashboard statistics");
                }
            })
            .WithName("GetAdminDashboardStats")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter estatísticas do dashboard administrativo",
                Description = "Retorna estatísticas gerais para o dashboard administrativo.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para relatório de vendas
            app.MapGet("/admin/sales/report", async (
                DateTime? startDate,
                DateTime? endDate,
                IAdminStatsService adminStatsService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var report = await adminStatsService.GetSalesReportAsync(startDate, endDate);
                    return Results.Ok(report);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving sales report");
                    return Results.Problem("Error retrieving sales report");
                }
            })
            .WithName("GetSalesReport")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter relatório de vendas",
                Description = "Retorna relatório detalhado de vendas para um período específico.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para rifas mais vendidas
            app.MapGet("/admin/rifas/top-selling", async (
                int count,
                IAdminStatsService adminStatsService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var topRifas = await adminStatsService.GetTopSellingRifasAsync(count);
                    return Results.Ok(topRifas);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving top selling rifas");
                    return Results.Problem("Error retrieving top selling rifas");
                }
            })
            .WithName("GetTopSellingRifas")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter rifas mais vendidas",
                Description = "Retorna as rifas com maior número de vendas.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para vendas recentes de tickets
            app.MapGet("/admin/tickets/recent-sales", async (
                int count,
                IAdminStatsService adminStatsService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var recentSales = await adminStatsService.GetRecentTicketSalesAsync(count);
                    return Results.Ok(recentSales);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving recent ticket sales");
                    return Results.Problem("Error retrieving recent ticket sales");
                }
            })
            .WithName("GetRecentTicketSales")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter vendas recentes de tickets",
                Description = "Retorna as vendas mais recentes de tickets.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para sorteios próximos
            app.MapGet("/admin/draws/upcoming", async (
                int count,
                IAdminStatsService adminStatsService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var upcomingDraws = await adminStatsService.GetUpcomingDrawsAsync(count);
                    return Results.Ok(upcomingDraws);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving upcoming draws");
                    return Results.Problem("Error retrieving upcoming draws");
                }
            })
            .WithName("GetUpcomingDraws")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter sorteios próximos",
                Description = "Retorna os sorteios que ocorrerão em breve.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });
        }
    }
}