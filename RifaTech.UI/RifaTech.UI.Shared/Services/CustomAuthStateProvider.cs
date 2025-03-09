// RifaTech.UI.Shared/Services/CustomAuthStateProvider.cs

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using RifaTech.UI.Shared.Models;
using RifaTech.UI.Shared.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        HttpClient http,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _http = http;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            _logger.LogInformation("Verificando estado de autenticação");
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("Token não encontrado, usuário não autenticado");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);

            // Log de diagnóstico para as claims
            foreach (var claim in claims)
            {
                _logger.LogDebug($"Claim encontrada: {claim.Type} = {claim.Value}");
            }

            var expiry = claims.FirstOrDefault(c => c.Type.Equals("exp"))?.Value;

            if (expiry != null && long.TryParse(expiry, out long expiryTime))
            {
                var expiryDateUtc = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;

                if (expiryDateUtc <= DateTime.UtcNow)
                {
                    _logger.LogInformation("Token expirado, tentando refresh");
                    return await RefreshTokenAsync();
                }
            }

            // Adicionar token ao cabeçalho de autorização
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            // Verificar explicitamente se o usuário tem role Admin
            bool isAdmin = user.IsInRole("Admin");
            _logger.LogInformation($"Usuário autenticado com sucesso. IsAdmin: {isAdmin}");

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar estado de autenticação");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private async Task<AuthenticationState> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogInformation("Refresh token não encontrado, realizando logout");
                await LogoutAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var response = await _http.PostAsync($"api/manage/refresh-token?refreshToken={refreshToken}", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

                if (result.Flag)
                {
                    _logger.LogInformation("Refresh token bem-sucedido, atualizando tokens");
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

                    var claims = ParseClaimsFromJwt(result.Token);
                    _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                    // Log de diagnóstico para as claims após refresh
                    foreach (var claim in claims)
                    {
                        _logger.LogDebug($"Claim após refresh: {claim.Type} = {claim.Value}");
                    }

                    var identity = new ClaimsIdentity(claims, "jwt");
                    var user = new ClaimsPrincipal(identity);

                    return new AuthenticationState(user);
                }
                else
                {
                    _logger.LogWarning($"Falha no refresh: {result.Message}");
                }
            }
            else
            {
                _logger.LogWarning($"Resposta de refresh inválida: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token");
        }

        await LogoutAsync();
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("Realizando logout");
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");

        _http.DefaultRequestHeaders.Authorization = null;

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);

        // Log de diagnóstico para as claims na autenticação
        foreach (var claim in claims)
        {
            _logger.LogDebug($"Claim na autenticação: {claim.Type} = {claim.Value}");
        }

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

        // Verificar explicitamente se o usuário tem role Admin
        bool isAdmin = authenticatedUser.IsInRole("Admin");
        _logger.LogInformation($"Notificando autenticação do usuário. IsAdmin: {isAdmin}");

        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var claims = token.Claims.ToList();

            // Verificar se há claim de role
            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").ToList();

            if (!roleClaims.Any())
            {
                _logger.LogWarning("Não foram encontradas claims de role no token");
            }
            else
            {
                foreach (var roleClaim in roleClaims)
                {
                    _logger.LogInformation($"Role encontrada: {roleClaim.Value}");
                }
            }

            return claims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar claims do JWT");
            return new List<Claim>();
        }
    }
}



