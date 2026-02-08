using Microsoft.OpenApi.Models;
using RifaTech.API.Services;
using RifaTech.DTOs.DTOs.AdminDashboard;

namespace RifaTech.API.Endpoints
{
    public static class DrawManagementEndpoints
    {
        public static void RegisterDrawManagementEndpoints(this IEndpointRouteBuilder app)
        {
            // Endpoint para executar um sorteio
            app.MapPost("/admin/draws/execute/{rifaId}", async (
                Guid rifaId,
                IDrawManagementService drawManagementService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var result = await drawManagementService.ExecuteDrawAsync(rifaId);
                    return Results.Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error executing draw for rifa {rifaId}");
                    return Results.Problem($"Error executing draw: {ex.Message}");
                }
            })
            .WithName("ExecuteDraw")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Executar sorteio",
                Description = "Executa um sorteio para uma rifa específica e retorna o resultado.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para obter histórico de sorteios
            app.MapGet("/admin/draws/history", async (
                int count,
                IDrawManagementService drawManagementService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var history = await drawManagementService.GetDrawHistoryAsync(count);
                    return Results.Ok(history);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving draw history");
                    return Results.Problem($"Error retrieving draw history: {ex.Message}");
                }
            })
            .WithName("GetDrawHistory")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter histórico de sorteios",
                Description = "Retorna o histórico dos últimos sorteios realizados.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para obter prévia de um sorteio
            app.MapGet("/admin/draws/preview/{rifaId}", async (
                Guid rifaId,
                IDrawManagementService drawManagementService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var preview = await drawManagementService.GetDrawPreviewAsync(rifaId);
                    return Results.Ok(preview);
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error retrieving draw preview for rifa {rifaId}");
                    return Results.Problem($"Error retrieving draw preview: {ex.Message}");
                }
            })
            .WithName("GetDrawPreview")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter prévia de sorteio",
                Description = "Retorna dados de prévia para um sorteio específico, incluindo participantes e chances de vitória.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para agendar um sorteio
            app.MapPost("/admin/draws/schedule", async (
                ScheduleDrawRequest request,
                IDrawManagementService drawManagementService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var success = await drawManagementService.ScheduleDrawAsync(request.RifaId, request.DrawDateTime);
                    return Results.Ok(new { message = "Draw scheduled successfully" });
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error scheduling draw for rifa {request.RifaId}");
                    return Results.Problem($"Error scheduling draw: {ex.Message}");
                }
            })
            .WithName("ScheduleDraw")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Agendar sorteio",
                Description = "Agenda um sorteio para uma data e hora específicas.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });

            // Endpoint para cancelar um sorteio
            app.MapPost("/admin/draws/cancel/{rifaId}", async (
                Guid rifaId,
                IDrawManagementService drawManagementService,
                ILogger<Program> logger) =>
            {
                try
                {
                    var success = await drawManagementService.CancelDrawAsync(rifaId);
                    return Results.Ok(new { message = "Draw canceled successfully" });
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning(ex.Message);
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error canceling draw for rifa {rifaId}");
                    return Results.Problem($"Error canceling draw: {ex.Message}");
                }
            })
            .WithName("CancelDraw")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Acesso somente para administradores
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Cancelar sorteio",
                Description = "Cancela um sorteio agendado.",
                Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
            });
        }
    }
}