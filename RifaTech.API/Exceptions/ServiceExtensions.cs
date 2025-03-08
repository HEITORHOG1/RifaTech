using RifaTech.API.Repositories;
using RifaTech.API.Services;
using RifaTech.DTOs.Contracts;

namespace RifaTech.API.Exceptions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Repository services
            services.AddTransient<IRifaService, RifaService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IExtraNumberService, ExtraNumberService>();
            services.AddTransient<IDrawService, DrawService>();
            services.AddTransient<IUserAccount, AccountService>();
            services.AddTransient<IClienteService, ClienteService>();
            services.AddTransient<IUnpaidRifaService, UnpaidRifaService>();

            // Add CompraRapida service
            services.AddScoped<ICompraRapidaService, CompraRapidaService>();
            // Add cache service (this is now done in Program.cs as a singleton)
            services.AddSingleton<ICacheService, MemoryCacheService>();

            services.AddScoped<INotificationService, EmailNotificationService>();
        }
    }
}