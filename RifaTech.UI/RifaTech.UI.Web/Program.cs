using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using MudBlazor.Services;
using RifaTech.UI.Shared.Config;
using RifaTech.UI.Shared.Services;
using RifaTech.UI.Web.Components;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Registrar Local Storage
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

// Configurar Autenticação e Autorização
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RifaTech",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RifaTechUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuaChaveSecretaTemporariaDeveSerSubstituida"))
        };
    });
builder.Services.AddAuthorization();

// Configurar serviços de autenticação personalizada
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// Registrar os serviços de forma correta (evitar duplicatas)
builder.Services.AddScoped<ClienteRecorrenteService>();
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

// Importante: Adicionar middleware de autenticação e autorização na ordem correta
app.UseAuthentication(); // Adicionar isso
app.UseAuthorization();  // Já existente ou adicionar se não tiver

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(RifaTech.UI.Shared._Imports).Assembly,
        typeof(RifaTech.UI.Web.Client._Imports).Assembly);

app.Run();