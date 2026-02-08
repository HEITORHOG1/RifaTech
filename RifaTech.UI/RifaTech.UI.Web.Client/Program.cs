using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using RifaTech.UI.Shared.Config;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Client.Services;
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Registrar Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ClienteRecorrenteService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IStorageService, BrowserStorageService>();

// Add device-specific services
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Buscar a URL da API do endpoint de configuração do servidor
string? browserApiUrl = null;
try
{
    using var tempHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var configResponse = await tempHttp.GetFromJsonAsync<JsonElement>("/api/client-config");
    browserApiUrl = configResponse.GetProperty("apiBaseUrl").GetString();
    Console.WriteLine($"WASM: URL da API obtida do servidor: {browserApiUrl}");
}
catch (Exception ex)
{
    Console.WriteLine($"WASM: Falha ao obter config do servidor: {ex.Message}");
}

var apiUrl = browserApiUrl
    ?? builder.Configuration["ApiSettings:BaseUrl"]
    ?? AppConfig.Api.BaseUrl;

Console.WriteLine($"WASM: Configurando HttpClient com BaseAddress: {apiUrl}");

// Configurar HttpClient
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiUrl)
    };
    return httpClient;
});

await builder.Build().RunAsync();