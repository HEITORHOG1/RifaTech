using RifaTech.API.Repositories;
using RifaTech.DTOs.Contracts;

namespace RifaTech.API
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IRifaService, RifaService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IExtraNumberService, ExtraNumberService>();
            services.AddTransient<IDrawService, DrawService>();
            services.AddTransient<IUserAccount, AccountService>();
            services.AddTransient<IClienteService, ClienteService>();
            services.AddTransient<IUnpaidRifaService, UnpaidRifaService>();
        }
    }
}