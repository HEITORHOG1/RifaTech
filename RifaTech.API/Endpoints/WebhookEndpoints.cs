using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using RifaTech.API.Entities;
using RifaTech.API.Services;

namespace RifaTech.API.Endpoints
{
    public static class WebhookEndpoints
    {
        public static void RegisterWebhookEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/webhooks/mercadopago", async (
                [FromBody] WebhookPaymentNotification notification,
                IWebhookService webhookService,
                ILogger<Program> logger) =>
            {
                try
                {
                    logger.LogInformation($"Webhook received from Mercado Pago: {notification?.Type}");

                    await webhookService.ProcessPaymentWebhookAsync(notification);

                    return Results.Ok(new { status = "success" });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Mercado Pago webhook");
                    return Results.Problem("Error processing webhook", statusCode: 500);
                }
            })
            .WithName("MercadoPagoWebhook")
            .AllowAnonymous() // Webhook must be accessible without authentication
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Webhook do Mercado Pago",
                Description = "Recebe notificações do Mercado Pago sobre atualizações de pagamento.",
                Tags = new List<OpenApiTag> { new() { Name = "Webhooks" } }
            });
        }
    }
}