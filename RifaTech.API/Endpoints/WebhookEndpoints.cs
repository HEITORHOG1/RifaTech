using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using RifaTech.API.Entities;
using RifaTech.API.Services;
using System.Security.Cryptography;
using System.Text;

namespace RifaTech.API.Endpoints
{
    public static class WebhookEndpoints
    {
        public static void RegisterWebhookEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/webhooks/mercadopago", async (
                HttpContext httpContext,
                [FromBody] WebhookPaymentNotification notification,
                IWebhookService webhookService,
                IConfiguration configuration,
                ILogger<Program> logger) =>
            {
                // ==============================================
                // WEBHOOK SIGNATURE VALIDATION
                // Protects against forged webhook requests.
                // ==============================================
                var webhookSecret = configuration["MercadoPago:WebhookSecret"];
                if (!string.IsNullOrEmpty(webhookSecret) && webhookSecret != "CHANGE_ME_WEBHOOK_SECRET")
                {
                    var xSignature = httpContext.Request.Headers["x-signature"].FirstOrDefault();
                    var xRequestId = httpContext.Request.Headers["x-request-id"].FirstOrDefault();

                    if (string.IsNullOrEmpty(xSignature))
                    {
                        logger.LogWarning("Webhook rejected: missing x-signature header");
                        return Results.Unauthorized();
                    }

                    // Parse x-signature to extract ts and v1
                    var signatureParts = xSignature.Split(',')
                        .Select(p => p.Trim().Split('='))
                        .Where(p => p.Length == 2)
                        .ToDictionary(p => p[0], p => p[1]);

                    if (signatureParts.TryGetValue("ts", out var ts) &&
                        signatureParts.TryGetValue("v1", out var v1))
                    {
                        // Build the manifest string for HMAC validation
                        var dataId = notification?.Data?.Id.ToString() ?? "";
                        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";

                        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
                        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest));
                        var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                        if (computedSignature != v1)
                        {
                            logger.LogWarning("Webhook rejected: invalid signature for request {RequestId}", xRequestId);
                            return Results.Unauthorized();
                        }
                    }
                }

                logger.LogInformation("Webhook received from Mercado Pago: Type={Type}, Action={Action}",
                    notification?.Type, notification?.Action);

                await webhookService.ProcessPaymentWebhookAsync(notification!);

                return Results.Ok(new { status = "success" });
            })
            .WithName("MercadoPagoWebhook")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Webhook do Mercado Pago",
                Description = "Recebe notificações do Mercado Pago sobre atualizações de pagamento.",
                Tags = new List<OpenApiTag> { new() { Name = "Webhooks" } }
            });
        }
    }
}