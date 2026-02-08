using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

public static class PaymentEndpoints
{
    public static void RegisterPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payments")
            .WithTags("Pagamentos");

        // Admin only — list all payments
        group.MapGet("/", async (IPaymentService paymentService, ILogger<Program> logger) =>
        {
            var payments = await paymentService.GetAllPaymentsAsync();
            logger.LogInformation("Retrieved {Count} payments", payments.Count());
            return Results.Ok(payments);
        })
        .WithName("GetAllPayments")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Listar todos os pagamentos",
            Description = "Retorna uma lista de todos os pagamentos realizados."
        });

        // Admin only — get payment by ID
        group.MapGet("/{id:guid}", async (Guid id, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            var payment = await paymentService.GetPaymentByIdAsync(id);
            if (payment is null)
            {
                logger.LogWarning("Payment {PaymentId} not found", id);
                return Results.NotFound();
            }

            logger.LogInformation("Retrieved payment {PaymentId}", id);
            return Results.Ok(payment);
        })
        .WithName("GetPaymentById")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Obter detalhes de um pagamento",
            Description = "Retorna os detalhes de um pagamento específico pelo ID."
        });

        // Admin only — create payment
        group.MapPost("/", async (PaymentDTO payment, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            var createdPayment = await paymentService.ProcessPaymentAsync(payment);
            logger.LogInformation("Created payment {PaymentId} for ticket {TicketId}",
                createdPayment.Id, createdPayment.TicketId);
            return Results.Created($"/payments/{createdPayment.Id}", createdPayment);
        })
        .WithName("CreatePayment")
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Registrar um novo pagamento",
            Description = "Registra um novo pagamento para um ticket."
        });

        // Public — check payment status
        group.MapGet("/status/{paymentId:guid}", async (Guid paymentId, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            var paymentStatus = await paymentService.CheckPaymentStatusAsync(paymentId);
            logger.LogInformation("Checked status for payment {PaymentId}: Status={Status}",
                paymentId, paymentStatus.Status);
            return Results.Ok(paymentStatus);
        })
        .WithName("CheckPaymentStatus")
        .AllowAnonymous()
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Verificar o status do pagamento",
            Description = "Verifica o status atual de um pagamento específico."
        });

        // Public — PIX payment
        group.MapPost("/pix", async ([FromBody] PixPaymentRequest request, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            var createdPayment = await paymentService.IniciarPagamentoPix(
                request.RifaId,
                request.Quantidade,
                request.ValorTotal,
                request.ClienteId);

            logger.LogInformation("Created PIX payment {PaymentId} for rifa {RifaId}, amount {Amount}",
                createdPayment.Id, request.RifaId, request.ValorTotal);
            return Results.Created($"/payments/{createdPayment.Id}", createdPayment);
        })
        .WithName("GerarPaymentPix")
        .AllowAnonymous()
        .RequireRateLimiting("PaymentEndpoints")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Gerar pagamento Pix",
            Description = "Gera um pagamento Pix para um ticket."
        });
    }
}