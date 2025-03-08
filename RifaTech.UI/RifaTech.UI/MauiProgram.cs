using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using RifaTech.UI.Shared.Config;
using RifaTech.UI.Shared.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using ILocalStorageService = RifaTech.UI.Shared.Services.ILocalStorageService;

namespace RifaTech.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registrar Local Storage
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

            // Registrar MudBlazor
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

            // Configurar HttpClient usando AppConfig
            builder.Services.AddScoped(sp =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(AppConfig.Api.BaseUrl) // URL da API centralizada
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
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
            builder.Services.AddScoped<ClienteRecorrenteService>();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}