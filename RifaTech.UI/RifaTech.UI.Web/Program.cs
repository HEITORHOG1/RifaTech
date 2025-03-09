using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Services;
using RifaTech.UI.Shared.Config;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Registrar Local Storage (remover duplicata)
builder.Services.AddBlazoredLocalStorage();

// Configurar HttpClient usando AppConfig
builder.Services.AddScoped(sp =>
{
    // Obter a URL base da configuração
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? AppConfig.Api.BaseUrl;

    // Log para depuração
    Console.WriteLine($"Configurando HttpClient com BaseAddress: {apiUrl}");

    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiUrl)
    };

    return httpClient;
});

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

// Configurar Autenticação
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// Registrar os serviços de forma correta (evitar duplicatas)

builder.Services.AddScoped<ClienteRecorrenteService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IStorageService, BrowserStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(RifaTech.UI.Shared._Imports).Assembly,
        typeof(RifaTech.UI.Web.Client._Imports).Assembly);

app.Run();