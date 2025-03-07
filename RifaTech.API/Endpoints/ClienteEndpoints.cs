using Microsoft.OpenApi.Models;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;

public static class ClienteEndpoints
{
    public static void RegisterClienteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/clientes", async (IClienteService clienteService, ILogger<Program> logger) =>
        {
            try
            {
                var clientes = await clienteService.GetAllClientesAsync();
                logger.LogInformation("Retrieved all clientes successfully");
                return Results.Ok(clientes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving clientes");
                throw;
            }
        })
        .WithName("GetAllClientes")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Listar todos os clientes",
            Description = "Retorna todos os clientes.",
            Tags = new List<OpenApiTag> { new() { Name = "Clientes" } }
        });

        app.MapGet("/clientes/{id}", async (Guid id, IClienteService clienteService, ILogger<Program> logger) =>
        {
            try
            {
                var cliente = await clienteService.GetClienteByIdAsync(id);
                if (cliente != null)
                {
                    logger.LogInformation($"Retrieved cliente with ID {id} successfully");
                    return Results.Ok(cliente);
                }
                else
                {
                    logger.LogWarning($"Cliente with ID {id} not found");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving cliente with ID {id}");
                throw;
            }
        })
        .WithName("GetClienteById")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Obter detalhes de um cliente",
            Description = "Retorna os detalhes de um cliente específico pelo ID.",
            Tags = new List<OpenApiTag> { new() { Name = "Clientes" } }
        });

        app.MapPost("/clientes", async (ClienteDTO clienteDto, IClienteService clienteService, ILogger<Program> logger) =>
        {
            try
            {
                var createdCliente = await clienteService.CreateClienteAsync(clienteDto);
                logger.LogInformation($"Created cliente with ID {createdCliente.Id} successfully");
                return Results.Created($"/clientes/{createdCliente.Id}", createdCliente);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating cliente");
                throw;
            }
        })
        .WithName("CreateCliente")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Criar um novo cliente",
            Description = "Cria um novo cliente.",
            Tags = new List<OpenApiTag> { new() { Name = "Clientes" } }
        });

        app.MapPut("/clientes/{id}", async (Guid id, ClienteDTO updatedClienteDto, IClienteService clienteService, ILogger<Program> logger) =>
        {
            try
            {
                if (await clienteService.UpdateClienteAsync(id, updatedClienteDto))
                {
                    logger.LogInformation($"Updated cliente with ID {id} successfully");
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning($"Cliente with ID {id} not found for update");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating cliente with ID {id}");
                throw;
            }
        })
        .WithName("UpdateCliente")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Atualizar um cliente",
            Description = "Atualiza informações de um cliente específico.",
            Tags = new List<OpenApiTag> { new() { Name = "Clientes" } }
        });

        app.MapDelete("/clientes/{id}", async (Guid id, IClienteService clienteService, ILogger<Program> logger) =>
        {
            try
            {
                if (await clienteService.DeleteClienteAsync(id))
                {
                    logger.LogInformation($"Deleted cliente with ID {id} successfully");
                    return Results.Ok();
                }
                else
                {
                    logger.LogWarning($"Cliente with ID {id} not found for deletion");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting cliente with ID {id}");
                throw;
            }
        })
        .WithName("DeleteCliente")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Deletar um cliente",
            Description = "Remove um cliente existente.",
            Tags = new List<OpenApiTag> { new() { Name = "Clientes" } }
        });

        //criar o endpoint GetClienteByEmailOrPhoneNumberOrCPF
        app.MapGet("/clientes/{email}/{phoneNumber}/{cpf}", async (string email, string phoneNumber, string cpf, IClienteService clienteService, ILogger<Program> logger) =>
        {
            try
            {
                var cliente = await clienteService.GetClienteByEmailOrPhoneNumberOrCPFAsync(email, phoneNumber, cpf);
                if (cliente != null)
                {
                    logger.LogInformation($"Retrieved cliente with email {email} or phoneNumber {phoneNumber} or cpf {cpf} successfully");
                    return Results.Ok(cliente);
                }
                else
                {
                    logger.LogWarning($"Cliente with email {email} or phoneNumber {phoneNumber} or cpf {cpf} not found");
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving cliente with email {email} or phoneNumber {phoneNumber} or cpf {cpf}");
                throw;
            }
        }).WithName("")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Obter detalhes de um cliente",
            Description = "Retorna os detalhes de um cliente específico pelo email, phoneNumber ou cpf.",
            Tags = new List<OpenApiTag> { new() { Name = "Clientes" } }
        });
    }
}