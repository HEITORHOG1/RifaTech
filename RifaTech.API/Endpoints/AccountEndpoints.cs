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
                .WithOpenApi(x => new OpenApiOperation(x)
                {
                    Summary = "Obter Todos os Usuários",
                    Description = "Retorna uma lista de todos os usuários.",
                    Tags = new List<OpenApiTag> { new() { Name = "Manage" } }
                });
        }
    }
}