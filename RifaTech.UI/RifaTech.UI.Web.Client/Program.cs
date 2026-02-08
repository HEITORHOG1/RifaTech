using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using RifaTech.UI.Shared.Config;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Client.Services;

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

// Configurar HttpClient
builder.Services.AddScoped(sp =>
{
    // Usar a URL base da API para cliente WASM
    var apiUrl = AppConfig.Api.BaseUrl;
    Console.WriteLine($"WASM: Configurando HttpClient com BaseAddress: {apiUrl}");

    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiUrl)
    };

    return httpClient;
});

await builder.Build().RunAsync();