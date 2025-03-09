// RifaTech.UI.Shared/Services/CustomAuthStateProvider.cs

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using RifaTech.UI.Shared.Config;
using RifaTech.UI.Shared.Helpers;
using RifaTech.UI.Shared.Models;
using RifaTech.UI.Shared.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IStorageService _localStorage;
    private readonly HttpClient _http;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private bool _initialized = false;
    private ClaimsPrincipal _anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
    private Task<AuthenticationState> _cachedAuthState;
    private DateTime _lastRefreshCheck = DateTime.MinValue;
    private const int MinRefreshIntervalSeconds = 10; // Evitar múltiplas verificações em sequência

    public CustomAuthStateProvider(
        IStorageService localStorage,
        HttpClient http,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _http = http;
        _logger = logger;

        // Começamos com um estado anônimo
        _cachedAuthState = Task.FromResult(new AuthenticationState(_anonymousUser));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Se ainda não inicializamos ou já passaram mais de MinRefreshIntervalSeconds desde a última verificação,
        // tentamos buscar o estado atual
        if (!_initialized || (DateTime.Now - _lastRefreshCheck).TotalSeconds > MinRefreshIntervalSeconds)
        {
            try
            {
                await _semaphore.WaitAsync();
                await InitializeAuthenticationStateAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return await _cachedAuthState;
    }
    public async Task InitializeAuthenticationStateAsync()
    {
        try
        {
            _logger.LogInformation("Verificando estado de autenticação");
            _lastRefreshCheck = DateTime.Now;

            var token = await _localStorage.GetItemAsync<string>(AppConfig.LocalStorage.AuthTokenKey);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("Token não encontrado, usuário não autenticado");
                _cachedAuthState = Task.FromResult(new AuthenticationState(_anonymousUser));
                _initialized = true;
                SetHttpAuthHeader(null);
                return;
            }

            var claims = ParseClaimsFromJwt(token);

            // Verificar se o token expirou
            if (IsTokenExpired(claims))
            {
                _logger.LogInformation("Token expirado, tentando refresh");
                var refreshed = await RefreshTokenInternalAsync();
                if (!refreshed)
                {
                    // Se não conseguimos fazer refresh, o usuário está deslogado
                    _cachedAuthState = Task.FromResult(new AuthenticationState(_anonymousUser));
                    _initialized = true;
                    SetHttpAuthHeader(null);
                    return;
                }

                // Recebemos um novo token, buscamos suas claims
                token = await _localStorage.GetItemAsync<string>(AppConfig.LocalStorage.AuthTokenKey);
                claims = ParseClaimsFromJwt(token);
            }

            // Configurar authorization header
            SetHttpAuthHeader(token);

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _cachedAuthState = Task.FromResult(new AuthenticationState(user));
            _initialized = true;

            _logger.LogInformation($"Usuário autenticado com sucesso. IsAdmin: {user.IsInRole("Admin")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar estado de autenticação");
            _cachedAuthState = Task.FromResult(new AuthenticationState(_anonymousUser));
            _initialized = true;
            SetHttpAuthHeader(null);
        }
    }

    private bool IsTokenExpired(IEnumerable<Claim> claims)
    {
        try
        {
            var expiry = claims.FirstOrDefault(c => c.Type.Equals("exp"))?.Value;

            if (expiry != null && long.TryParse(expiry, out long expiryTime))
            {
                var expiryDateUtc = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;
                // Renovar se faltar menos de 5 minutos para expirar
                return expiryDateUtc <= DateTime.UtcNow.AddMinutes(5);
            }

            return true; // Se não conseguimos verificar, consideramos expirado por segurança
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar expiração do token");
            return true;
        }
    }

    private async Task<bool> RefreshTokenInternalAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>(AppConfig.LocalStorage.RefreshTokenKey);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogInformation("Refresh token não encontrado, realizando logout");
                await LogoutAsync();
                return false;
            }

            // Criar novo HttpClient para evitar problemas com o header de autorização
            using var client = new HttpClient { BaseAddress = _http.BaseAddress };
            var response = await client.PostAsync($"{AppConfig.Api.Endpoints.RefreshToken}?refreshToken={refreshToken}", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

                if (result.Flag)
                {
                    _logger.LogInformation("Refresh token bem-sucedido, atualizando tokens");
                    await _localStorage.SetItemAsync(AppConfig.LocalStorage.AuthTokenKey, result.Token);
                    await _localStorage.SetItemAsync(AppConfig.LocalStorage.RefreshTokenKey, result.RefreshToken);

                    // Também salvamos outras informações úteis
                    await _localStorage.SetItemAsync("userRole", result.Role);
                    await _localStorage.SetItemAsync("isAdmin", result.EhAdmin);

                    SetHttpAuthHeader(result.Token);
                    NotifyAuthenticationStateChanged(CreateAuthStateFromToken(result.Token));
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Falha no refresh: {result.Message}");
                    await LogoutAsync();
                    return false;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Resposta de refresh inválida: {response.StatusCode}, {errorContent}");
                await LogoutAsync();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token");
            await LogoutAsync();
            return false;
        }
    }

    private Task<AuthenticationState> CreateAuthStateFromToken(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
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

        try
        {
            await _localStorage.RemoveItemAsync(AppConfig.LocalStorage.AuthTokenKey);
            await _localStorage.RemoveItemAsync(AppConfig.LocalStorage.RefreshTokenKey);
            await _localStorage.RemoveItemAsync("userRole");
            await _localStorage.RemoveItemAsync("isAdmin");
            await _localStorage.RemoveItemAsync("userId");

            SetHttpAuthHeader(null);

            _cachedAuthState = Task.FromResult(new AuthenticationState(_anonymousUser));
            NotifyAuthenticationStateChanged(_cachedAuthState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout");
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Tentativa de autenticação com token vazio");
            return;
        }

        try
        {
            var claims = ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            SetHttpAuthHeader(token);

            _cachedAuthState = authState;
            NotifyAuthenticationStateChanged(authState);

            _logger.LogInformation("Autenticação do usuário notificada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao notificar autenticação do usuário");
        }
    }

    private void SetHttpAuthHeader(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var claims = JwtHelper.ParseClaimsFromJwt(jwt).ToList();

            // Verificar se há claim de role
            var roleClaims = claims.Where(c => c.Type == ClaimTypes.Role).ToList();

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

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}




