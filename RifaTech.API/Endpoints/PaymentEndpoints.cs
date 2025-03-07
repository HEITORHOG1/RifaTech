using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using RifaTech.API.Exceptions;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

public static class PaymentEndpoints
{
    public static void RegisterPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/payments", async (IPaymentService paymentService, ILogger<Program> logger) =>
        {
            try
            {
                var payments = await paymentService.GetAllPaymentsAsync();
                logger.LogInformation("Retrieved all payments successfully");
                return Results.Ok(payments);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving payments");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetAllPayments")
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Listar todos os pagamentos",
            Description = "Retorna uma lista de todos os pagamentos realizados.",
            Tags = new List<OpenApiTag> { new() { Name = "Pagamentos" } }
        });

        app.MapGet("/payments/{id}", async (Guid id, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            try
            {
                var payment = await paymentService.GetPaymentByIdAsync(id);
                if (payment != null)
                {
                    logger.LogInformation($"Retrieved payment with ID {id} successfully");
                    return Results.Ok(payment);
                }
                else
                {
                    logger.LogWarning($"Payment with ID {id} not found");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving payment with ID {id}");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("GetPaymentById")
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Obter detalhes de um pagamento",
            Description = "Retorna os detalhes de um pagamento específico pelo ID.",
            Tags = new List<OpenApiTag> { new() { Name = "Pagamentos" } }
        });

        app.MapPost("/payments", async (PaymentDTO payment, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            try
            {
                var createdPayment = await paymentService.ProcessPaymentAsync(payment);
                logger.LogInformation($"Created payment with ID {createdPayment.Id} successfully");
                return Results.Created($"/payment/{createdPayment.Id}", createdPayment);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing payment");
                throw; // O middleware de tratamento de exceções capturará isso
            }
        })
        .WithName("CreatePayment")
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,User" })
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Registrar um novo pagamento",
            Description = "Registra um novo pagamento para um ticket.",
            Tags = new List<OpenApiTag> { new() { Name = "Pagamentos" } }
        });

        app.MapGet("/payments/status/{paymentId}", async (Guid paymentId, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            try
            {
                var paymentStatus = await paymentService.CheckPaymentStatusAsync(paymentId);
                logger.LogInformation($"Checked status for payment ID {paymentId} successfully");
                return Results.Ok(paymentStatus);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error checking status for payment ID {paymentId}");
                return Results.Problem("Erro ao verificar o status do pagamento.");
            }
        })
       .WithName("CheckPaymentStatus")
       .WithOpenApi(x => new OpenApiOperation(x)
       {
           Summary = "Verificar o status do pagamento",
           Description = "Verifica o status atual de um pagamento específico.",
           Tags = new List<OpenApiTag> { new() { Name = "Pagamentos" } }
       });

        //chamar o metodo IniciarPagamentoPix
        app.MapPost("/payments/pix", async ([FromBody] PixPaymentRequest request, IPaymentService paymentService, ILogger<Program> logger) =>
        {
            try
            {
                var createdPayment = await paymentService.IniciarPagamentoPix(request.RifaId, request.Quantidade, request.ValorTotal, request.ClienteId);
                logger.LogInformation($"Created payment with ID {createdPayment.Id} successfully");
                return Results.Created($"/payment/{createdPayment.Id}", createdPayment);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing payment");
                throw; // The exception handling middleware will catch this
            }
        }).WithName("GerarPaymentPix").WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Gerar pagamento Pix",
            Description = "Gera um pagamento Pix para um ticket.",
            Tags = new List<OpenApiTag> { new() { Name = "Pagamentos" } }
        });
    }
}