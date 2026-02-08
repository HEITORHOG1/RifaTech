using Microsoft.OpenApi.Models;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;
using System.Security.Claims;

namespace RifaTech.API.Endpoints
{
    public static class AccountEndpoints
    {
        public static void RegisterAccountEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/manage/register", async (UserDTO userDTO, IUserAccount authService) =>
            {
                try
                {
                    var (token, refreshToken) = await authService.CreateAccount(userDTO);
                    return Results.Ok(new { Token = token, RefreshToken = refreshToken });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("Register")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Registrar Login",
                Description = "Retorna o Login Registrado.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            app.MapPost("/manage/login", async (LoginDTO registerDto, IUserAccount authService) =>
            {
                var result = await authService.LoginAccount(registerDto);

                return Results.Ok(result);
            })
            .WithName("Login")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Logar no Sistema",
                Description = "Retorna o Login o Token.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            app.MapGet("/manage/info", async (HttpContext httpContext, IUserAccount authService) =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var userInfo = await authService.GetUserInfo(userId);
                    return userInfo != null ? Results.Ok(userInfo) : Results.NotFound("User not found");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetUserInfo")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter Informações do Usuário",
                Description = "Retorna informações do usuário autenticado.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            app.MapPost("/manage/refresh-token", async (string refreshToken, IUserAccount authService) =>
            {
                var result = await authService.RefreshTokenAsync(refreshToken);

                return result.Flag ? Results.Ok(result) : Results.Unauthorized();
            })
            .WithName("RefreshToken")
            .AllowAnonymous()
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Atualizar Token de Acesso",
                Description = "Atualiza o token de acesso usando um refresh token.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            app.MapGet("/manage/users", async (IUserAccount authService) =>
            {
                try
                {
                    var users = await authService.GetAllUsersAsync();
                    return Results.Ok(users);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetAllUsers")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter Todos os Usuários",
                Description = "Retorna uma lista de todos os usuários.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            // NOVO ENDPOINT: Obter um usuário específico por ID
            app.MapGet("/manage/users/{id}", async (string id, IUserAccount authService) =>
            {
                try
                {
                    var user = await authService.GetUserByIdAsync(id);
                    return user != null ? Results.Ok(user) : Results.NotFound($"Usuário com ID {id} não encontrado");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetUserById")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Obter Usuário por ID",
                Description = "Retorna um usuário específico pelo ID.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            // NOVO ENDPOINT: Atualizar um usuário existente
            app.MapPut("/manage/users/{id}", async (string id, UserDTO userDTO, IUserAccount authService) =>
            {
                try
                {
                    var updated = await authService.UpdateUserAsync(id, userDTO);
                    return updated != null ?
                        Results.Ok(updated) :
                        Results.NotFound($"Usuário com ID {id} não encontrado");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("UpdateUser")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Atualizar Usuário",
                Description = "Atualiza os dados de um usuário existente.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            // NOVO ENDPOINT: Excluir um usuário
            app.MapDelete("/manage/users/{id}", async (string id, IUserAccount authService) =>
            {
                try
                {
                    var result = await authService.DeleteUserAsync(id);
                    return result ?
                        Results.Ok(new { success = true, message = "Usuário excluído com sucesso" }) :
                        Results.NotFound($"Usuário com ID {id} não encontrado");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("DeleteUser")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Excluir Usuário",
                Description = "Remove um usuário existente.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });

            // NOVO ENDPOINT: Atualizar papel/função de um usuário
            app.MapPut("/manage/users/{id}/role", async (string id, RoleUpdateDTO roleUpdate, IUserAccount authService) =>
            {
                try
                {
                    var updated = await authService.UpdateUserRoleAsync(id, roleUpdate.Role);
                    return updated != null ?
                        Results.Ok(updated) :
                        Results.NotFound($"Usuário com ID {id} não encontrado");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("UpdateUserRole")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Atualizar Função do Usuário",
                Description = "Atualiza a função/papel de um usuário existente.",
                Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
            });
        }
    }
}