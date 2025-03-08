using RifaTech.API.Repositories;
using RifaTech.API.Services;
using RifaTech.DTOs.Contracts;

namespace RifaTech.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Repository services
            services.AddTransient<IRifaService, RifaService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddTransient<IPaymentService, Repositories.PaymentService>();
            services.AddTransient<IExtraNumberService, ExtraNumberService>();
            services.AddTransient<IDrawService, DrawService>();
            services.AddTransient<IUserAccount, AccountService>();
            services.AddTransient<IClienteService, ClienteService>();
            services.AddTransient<IUnpaidRifaService, UnpaidRifaService>();

            // Add CompraRapida service
            services.AddScoped<ICompraRapidaService, CompraRapidaService>();

            // Add cache service
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // Add notification services
            services.AddScoped<ITemplateEngine, TemplateEngine>();
            services.AddScoped<EmailNotificationService>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.AddScoped<INotificationService, MultiChannelNotificationService>();

            // Add background services
            services.AddHostedService<PaymentStatusVerificationService>();
            services.AddHostedService<NotificationBackgroundService>();

            // Add MercadoPago service
            services.AddScoped<IMercadoPagoService, MercadoPagoService>();

            // Add Webhook service
            services.AddScoped<IWebhookService, WebhookService>();

            // Add HttpClient for WhatsApp API
            services.AddHttpClient<IWhatsAppService, WhatsAppService>();
        }
    }
}