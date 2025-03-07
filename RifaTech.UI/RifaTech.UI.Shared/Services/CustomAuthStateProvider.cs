// RifaTech.UI.Shared/Services/CustomAuthStateProvider.cs

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using RifaTech.UI.Shared.Models;
using RifaTech.UI.Shared.Services;
using System.Net.Http.Json;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var expiry = claims.FirstOrDefault(c => c.Type.Equals("exp"))?.Value;

        if (expiry != null && long.TryParse(expiry, out long expiryTime))
        {
            var expiryDateUtc = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;

            if (expiryDateUtc <= DateTime.UtcNow)
            {
                // Token expirado, tentar refresh
                var authState = await RefreshTokenAsync();
                return authState;
            }
        }

        // Adicionar token ao cabeçalho de autorização
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }

    private async Task<AuthenticationState> RefreshTokenAsync()
    {
        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

        if (string.IsNullOrEmpty(refreshToken))
        {
            await LogoutAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var response = await _http.PostAsync($"api/manage/refresh-token?refreshToken={refreshToken}", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

                if (result.Flag)
                {
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

                    var claims = ParseClaimsFromJwt(result.Token);
                    _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
                }
            }
        }
        catch
        {
            // Em caso de falha, fazer logout
        }

        await LogoutAsync();
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");

        _http.DefaultRequestHeaders.Authorization = null;

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        base.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous))); // Use 'base'
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        base.NotifyAuthenticationStateChanged(authState); // Use 'base' para acessar o método protegido
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}


