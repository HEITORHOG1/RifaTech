using RifaTech.API.Entities.Notifications;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Services
{
    /// <summary>
    /// A notification service that combines multiple notification channels (email, WhatsApp)
    /// </summary>
    public class MultiChannelNotificationService : INotificationService
    {
        private readonly EmailNotificationService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<MultiChannelNotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _enableWhatsApp;

        public MultiChannelNotificationService(
            EmailNotificationService emailService,
            IWhatsAppService whatsAppService,
            ILogger<MultiChannelNotificationService> logger,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _logger = logger;
            _configuration = configuration;
            _enableWhatsApp = bool.Parse(_configuration["Notifications:EnableWhatsApp"] ?? "false");
        }

        public async Task SendNotificationAsync(NotificationBase notification)
        {
            // Always send email notification
            await _emailService.SendNotificationAsync(notification);
        }

        public async Task SendPurchaseConfirmationAsync(CompraRapidaResponseDTO compraResponse)
        {
            // Send email notification
            await _emailService.SendPurchaseConfirmationAsync(compraResponse);

            // Send WhatsApp notification if enabled and phone number is available
            if (_enableWhatsApp && !string.IsNullOrEmpty(compraResponse.Cliente?.PhoneNumber))
            {
                try
                {
                    await _whatsAppService.SendPurchaseConfirmationAsync(
                        compraResponse.Cliente.PhoneNumber,
                        compraResponse.Cliente.Name,
                        compraResponse.RifaNome,
                        compraResponse.ValorTotal,
                        compraResponse.NumerosGerados,
                        compraResponse.CodigoPix,
                        compraResponse.ExpiracaoPix);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending WhatsApp purchase confirmation");
                }
            }
        }

        public async Task SendPaymentConfirmationAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa, List<int> ticketNumbers)
        {
            // Send email notification
            await _emailService.SendPaymentConfirmationAsync(payment, cliente, rifa, ticketNumbers);

            // Send WhatsApp notification if enabled and phone number is available
            if (_enableWhatsApp && !string.IsNullOrEmpty(cliente?.PhoneNumber))
            {
                try
                {
                    await _whatsAppService.SendPaymentConfirmationAsync(
                        cliente.PhoneNumber,
                        cliente.Name,
                        rifa.Name,
                        payment.Amount,
                        ticketNumbers,
                        rifa.DrawDateTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending WhatsApp payment confirmation");
                }
            }
        }

        public async Task SendPaymentExpiredAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa)
        {
            // Send email notification
            await _emailService.SendPaymentExpiredAsync(payment, cliente, rifa);

            // WhatsApp notification for expired payments is optional and not implemented
        }

        public async Task SendDrawReminderAsync(RifaDTO rifa, ClienteDTO cliente, List<int> ticketNumbers)
        {
            // Send email notification
            await _emailService.SendDrawReminderAsync(rifa, cliente, ticketNumbers);

            // Send WhatsApp notification if enabled and phone number is available
            if (_enableWhatsApp && !string.IsNullOrEmpty(cliente?.PhoneNumber))
            {
                try
                {
                    await _whatsAppService.SendDrawReminderAsync(
                        cliente.PhoneNumber,
                        cliente.Name,
                        rifa.Name,
                        ticketNumbers,
                        rifa.DrawDateTime,
                        rifa.DrawDateTime - DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending WhatsApp draw reminder");
                }
            }
        }

        public async Task SendDrawResultAsync(RifaDTO rifa, DrawDTO draw, List<ClienteDTO> participants)
        {
            // Send email notification
            await _emailService.SendDrawResultAsync(rifa, draw, participants);

            // Send WhatsApp notification if enabled and phone number is available
            if (_enableWhatsApp)
            {
                int winningNumber = int.Parse(draw.WinningNumber);
                string winnerName = "Ganhador"; // We should get the real winner name

                foreach (var cliente in participants)
                {
                    if (!string.IsNullOrEmpty(cliente?.PhoneNumber))
                    {
                        try
                        {
                            await _whatsAppService.SendDrawResultAsync(
                                cliente.PhoneNumber,
                                cliente.Name,
                                rifa.Name,
                                winningNumber,
                                winnerName,
                                draw.DrawDateTime);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending WhatsApp draw result to {cliente.PhoneNumber}");
                        }
                    }
                }
            }
        }

        public async Task SendWinnerNotificationAsync(RifaDTO rifa, DrawDTO draw, ClienteDTO winner, int winningNumber)
        {
            // Send email notification
            await _emailService.SendWinnerNotificationAsync(rifa, draw, winner, winningNumber);

            // Send WhatsApp notification if enabled and phone number is available
            if (_enableWhatsApp && !string.IsNullOrEmpty(winner?.PhoneNumber))
            {
                try
                {
                    string contactInfo = "Entre em contato pelo telefone (XX) XXXX-XXXX ou pelo e-mail contato@rifatech.com para resgatar seu prêmio.";

                    await _whatsAppService.SendWinnerNotificationAsync(
                        winner.PhoneNumber,
                        winner.Name,
                        rifa.Name,
                        winningNumber,
                        rifa.PriceValue,
                        contactInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending WhatsApp winner notification");
                }
            }
        }
    }
}
