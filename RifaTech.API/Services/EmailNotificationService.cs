using RifaTech.API.Entities.Notifications;
using RifaTech.DTOs.DTOs;
using System.Net;
using System.Net.Mail;

namespace RifaTech.API.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly ITemplateEngine _templateEngine;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _enabled;

        public EmailNotificationService(
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger,
            ITemplateEngine templateEngine)
        {
            _configuration = configuration;
            _logger = logger;
            _templateEngine = templateEngine;

            // Carregando configurações de e-mail
            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _senderEmail = _configuration["Email:SenderEmail"];
            _senderName = _configuration["Email:SenderName"];
            _enabled = bool.Parse(_configuration["Email:Enabled"] ?? "true");
        }

        public async Task SendNotificationAsync(NotificationBase notification)
        {
            if (!_enabled)
            {
                _logger.LogInformation($"Email notifications are disabled. Would have sent {notification.NotificationType} to {notification.Recipient}");
                return;
            }

            if (string.IsNullOrEmpty(notification.Recipient))
            {
                _logger.LogWarning("E-mail do destinatário não fornecido, notificação não será enviada");
                return;
            }

            try
            {
                string template = GetTemplateByNotificationType(notification.NotificationType);
                string body = _templateEngine.RenderTemplate(template, notification);

                await SendEmailAsync(notification.Recipient, notification.Subject, body);
                _logger.LogInformation($"E-mail de {notification.NotificationType} enviado para {notification.Recipient}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail de {notification.NotificationType} para {notification.Recipient}");
            }
        }

        public async Task SendPurchaseConfirmationAsync(CompraRapidaResponseDTO compraResponse)
        {
            if (string.IsNullOrEmpty(compraResponse.Cliente?.Email))
            {
                _logger.LogWarning("E-mail do cliente não fornecido, notificação não será enviada");
                return;
            }

            var notification = new PurchaseConfirmationNotification
            {
                Recipient = compraResponse.Cliente.Email,
                ClienteName = compraResponse.Cliente.Name,
                RifaName = compraResponse.RifaNome,
                RifaId = compraResponse.RifaId,
                ValorTotal = compraResponse.ValorTotal,
                TicketNumbers = compraResponse.NumerosGerados,
                QrCodeBase64 = compraResponse.QrCodePix,
                QrCode = compraResponse.CodigoPix,
                ExpirationTime = compraResponse.ExpiracaoPix,
                PaymentStatus = compraResponse.StatusPagamento
            };

            await SendNotificationAsync(notification);
        }

        public async Task SendPaymentConfirmationAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa, List<int> ticketNumbers)
        {
            if (string.IsNullOrEmpty(cliente?.Email))
            {
                _logger.LogWarning("E-mail do cliente não fornecido, notificação não será enviada");
                return;
            }

            var notification = new PaymentConfirmationNotification
            {
                Recipient = cliente.Email,
                ClienteName = cliente.Name,
                RifaName = rifa.Name,
                RifaId = rifa.Id,
                ValorTotal = payment.Amount,
                TicketNumbers = ticketNumbers,
                DrawDateTime = rifa.DrawDateTime
            };

            await SendNotificationAsync(notification);
        }

        public async Task SendPaymentExpiredAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa)
        {
            if (string.IsNullOrEmpty(cliente?.Email))
            {
                _logger.LogWarning("E-mail do cliente não fornecido, notificação não será enviada");
                return;
            }

            var notification = new PaymentExpiredNotification
            {
                Recipient = cliente.Email,
                ClienteName = cliente.Name,
                RifaName = rifa.Name,
                RifaId = rifa.Id,
                ValorTotal = payment.Amount,
                ExpirationTime = payment.ExpirationTime ?? DateTime.UtcNow
            };

            await SendNotificationAsync(notification);
        }

        public async Task SendDrawReminderAsync(RifaDTO rifa, ClienteDTO cliente, List<int> ticketNumbers)
        {
            if (string.IsNullOrEmpty(cliente?.Email))
            {
                _logger.LogWarning("E-mail do cliente não fornecido, notificação não será enviada");
                return;
            }

            var notification = new DrawReminderNotification
            {
                Recipient = cliente.Email,
                ClienteName = cliente.Name,
                RifaName = rifa.Name,
                RifaId = rifa.Id,
                TicketNumbers = ticketNumbers,
                DrawDateTime = rifa.DrawDateTime,
                TimeRemaining = rifa.DrawDateTime - DateTime.UtcNow
            };

            await SendNotificationAsync(notification);
        }

        public async Task SendDrawResultAsync(RifaDTO rifa, DrawDTO draw, List<ClienteDTO> participantes)
        {
            foreach (var cliente in participantes)
            {
                if (string.IsNullOrEmpty(cliente?.Email))
                {
                    continue;
                }

                var notification = new DrawResultNotification
                {
                    Recipient = cliente.Email,
                    RifaName = rifa.Name,
                    RifaId = rifa.Id,
                    DrawDateTime = draw.DrawDateTime,
                    WinningNumber = int.Parse(draw.WinningNumber),
                    WinnerName = "Ganhador" // Temos que buscar o nome do cliente ganhador
                };

                await SendNotificationAsync(notification);
            }
        }

        public async Task SendWinnerNotificationAsync(RifaDTO rifa, DrawDTO draw, ClienteDTO winner, int winningNumber)
        {
            if (string.IsNullOrEmpty(winner?.Email))
            {
                _logger.LogWarning("E-mail do ganhador não fornecido, notificação não será enviada");
                return;
            }

            var notification = new WinnerNotification
            {
                Recipient = winner.Email,
                ClienteName = winner.Name,
                RifaName = rifa.Name,
                RifaId = rifa.Id,
                WinningNumber = winningNumber,
                PrizeValue = rifa.PriceValue,
                ContactInfo = "Entre em contato pelo telefone (XX) XXXX-XXXX ou pelo e-mail contato@rifatech.com para resgatar seu prêmio."
            };

            await SendNotificationAsync(notification);
        }

        private string GetTemplateByNotificationType(NotificationType type)
        {
            return type switch
            {
                NotificationType.PurchaseConfirmation => "PurchaseConfirmation",
                NotificationType.PaymentConfirmation => "PaymentConfirmation",
                NotificationType.PaymentExpired => "PaymentExpired",
                NotificationType.DrawReminder => "DrawReminder",
                NotificationType.DrawResult => "DrawResult",
                NotificationType.WinnerNotification => "WinnerNotification",
                _ => throw new ArgumentException($"Tipo de notificação não suportado: {type}")
            };
        }

        private async Task SendEmailAsync(string recipient, string subject, string body)
        {
            // Verifica se as configurações de e-mail estão presentes
            if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername) ||
                string.IsNullOrEmpty(_smtpPassword) || string.IsNullOrEmpty(_senderEmail))
            {
                _logger.LogWarning("Configurações de e-mail incompletas, e-mail não será enviado");
                return;
            }

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(recipient);

            await client.SendMailAsync(message);
        }
    }
}
}