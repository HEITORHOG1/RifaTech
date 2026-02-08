using Newtonsoft.Json;
using RifaTech.API.Entities;
using RifaTech.DTOs.Contracts;

namespace RifaTech.API.Services
{
    public class WebhookService : IWebhookService
    {
        private readonly ILogger<WebhookService> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly INotificationService _notificationService;

        public WebhookService(
            ILogger<WebhookService> logger,
            IPaymentService paymentService,
            IMercadoPagoService mercadoPagoService,
            INotificationService notificationService)
        {
            _logger = logger;
            _paymentService = paymentService;
            _mercadoPagoService = mercadoPagoService;
            _notificationService = notificationService;
        }

        public async Task ProcessPaymentWebhookAsync(WebhookPaymentNotification notification)
        {
            try
            {
                _logger.LogInformation($"Received payment webhook notification: {JsonConvert.SerializeObject(notification)}");

                // Validar o tipo de notificação
                if (notification.Type != "payment")
                {
                    _logger.LogWarning($"Ignoring non-payment webhook notification of type {notification.Type}");
                    return;
                }

                // Validar dados da notificação
                if (notification.Data == null || notification.Data.Id <= 0)
                {
                    _logger.LogWarning("Invalid payment webhook data");
                    return;
                }

                // Obter informações do pagamento do Mercado Pago
                var mpPayment = await _mercadoPagoService.GetPaymentStatusAsync(notification.Data.Id);

                if (mpPayment == null)
                {
                    _logger.LogWarning($"Payment {notification.Data.Id} not found in Mercado Pago");
                    return;
                }

                _logger.LogInformation($"Payment {notification.Data.Id} status: {mpPayment.Status}");

                // Buscar os pagamentos no banco de dados que têm este ID externo
                var payments = await _paymentService.GetPaymentsByExternalIdAsync(notification.Data.Id);

                if (payments == null || !payments.Any())
                {
                    _logger.LogWarning($"No matching payments found for external ID {notification.Data.Id}");
                    return;
                }

                // Atualizar cada pagamento
                foreach (var payment in payments)
                {
                    await _paymentService.CheckPaymentStatusAsync(payment.Id);
                    _logger.LogInformation($"Updated payment status for payment {payment.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment webhook");
                // Não propagar a exceção para que a resposta seja retornada ao Mercado Pago
            }
        }
    }
}