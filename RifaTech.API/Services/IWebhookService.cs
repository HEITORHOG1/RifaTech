using RifaTech.API.Entities;

namespace RifaTech.API.Services
{
    public interface IWebhookService
    {
        Task ProcessPaymentWebhookAsync(WebhookPaymentNotification notification);
    }
}