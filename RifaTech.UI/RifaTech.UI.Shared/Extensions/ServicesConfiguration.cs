using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;

namespace RifaTech.UI.Shared.Extensions
{
    public static class ServicesConfiguration
    {
        public static IServiceCollection AddRifaTechServices(this IServiceCollection services)
        {
            // Adicionar LocalStorage
            services.AddBlazoredLocalStorage();

            // Adicionar autenticação
            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            services.AddAuthorizationCore();

            return services;
        }
    }
}
