using FluentValidation;
using RifaTech.API.Repositories;
using RifaTech.API.Services;
using RifaTech.API.Validators;
using RifaTech.DTOs.Contracts;

namespace RifaTech.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // =========================================================
            // FluentValidation (auto-registers all validators in assembly)
            // =========================================================
            services.AddValidatorsFromAssemblyContaining<CompraRapidaValidator>(ServiceLifetime.Scoped);

            // =========================================================
            // Domain/Business services (Scoped — share DbContext lifetime)
            // =========================================================
            services.AddScoped<IRifaService, RifaService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IPaymentService, Repositories.PaymentService>();
            services.AddScoped<IExtraNumberService, ExtraNumberService>();
            services.AddScoped<IDrawService, DrawService>();
            services.AddScoped<IUserAccount, AccountService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IUnpaidRifaService, UnpaidRifaService>();
            services.AddScoped<ICompraRapidaService, CompraRapidaService>();

            // =========================================================
            // Cache (Singleton — thread-safe, shared across requests)
            // =========================================================
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // =========================================================
            // Notification services (Scoped)
            // =========================================================
            services.AddScoped<ITemplateEngine, TemplateEngine>();
            services.AddScoped<EmailNotificationService>();
            services.AddScoped<INotificationService, MultiChannelNotificationService>();

            // WhatsApp via HttpClientFactory (typed client)
            services.AddHttpClient<IWhatsAppService, WhatsAppService>();

            // =========================================================
            // Admin services (Scoped)
            // =========================================================
            services.AddScoped<IAdminStatsService, AdminStatsService>();
            services.AddScoped<IDrawManagementService, DrawManagementService>();
            services.AddScoped<ITicketSearchService, TicketSearchService>();

            // =========================================================
            // External integrations (Scoped)
            // =========================================================
            services.AddScoped<IMercadoPagoService, MercadoPagoService>();
            services.AddScoped<IWebhookService, WebhookService>();

            // =========================================================
            // Background services (Singleton by design)
            // =========================================================
            services.AddHostedService<PaymentStatusVerificationService>();
            services.AddHostedService<NotificationBackgroundService>();
        }
    }
}