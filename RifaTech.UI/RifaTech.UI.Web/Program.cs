﻿// RifaTech.UI.Web/Program.cs (Ajustado)

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Components;
using System.Text.Json;
using System.Text.Json.Serialization;
using ILocalStorageService = RifaTech.UI.Shared.Services.ILocalStorageService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Registrar MudBlazor
builder.Services.AddMudServices();

// Registrar Local Storage
builder.Services.AddBlazoredLocalStorage();

// Configurar HttpClient
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5024")
    };

    // Configurar serialização JSON para evitar problemas com referências circulares
    var options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true
    };

    return httpClient;
});

// Configurar Autenticação
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
// RifaTech.UI.Web/Program.cs (atualizado)

// Registrar serviços
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddAuthorizationCore();

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