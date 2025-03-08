using RifaTech.DTOs;
using RifaTech.DTOs.DTOs;
using System.Net;
using System.Net.Mail;

namespace RifaTech.API.Services
{
    public interface INotificationService
    {
        Task SendPurchaseConfirmationAsync(CompraRapidaResponseDTO compraResponse);
        Task SendPaymentConfirmationAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa, List<int> ticketNumbers);
    }

    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailNotificationService(
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Carregando configurações de e-mail
            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _senderEmail = _configuration["Email:SenderEmail"];
            _senderName = _configuration["Email:SenderName"];
        }

        public async Task SendPurchaseConfirmationAsync(CompraRapidaResponseDTO compraResponse)
        {
            if (string.IsNullOrEmpty(compraResponse.Cliente?.Email))
            {
                _logger.LogWarning("E-mail do cliente não fornecido, notificação não será enviada");
                return;
            }

            try
            {
                string subject = $"Confirmação de compra - {compraResponse.RifaNome}";

                // Corpo do e-mail em HTML
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4A6FE8; color: white; padding: 10px 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #888; }}
                        .info-box {{ background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }}
                        .qr-code {{ text-align: center; margin: 20px 0; }}
                        .cta-button {{ display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px; 
                                     text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                        .numbers {{ font-weight: bold; color: #4A6FE8; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Confirmação de Compra</h1>
                        </div>
                        <div class='content'>
                            <p>Olá <strong>{compraResponse.Cliente.Name}</strong>,</p>
                            
                            <p>Sua compra de {compraResponse.NumerosGerados.Count} número(s) para a rifa <strong>{compraResponse.RifaNome}</strong> foi registrada com sucesso!</p>
                            
                            <div class='info-box'>
                                <h3>Detalhes da compra:</h3>
                                <p><strong>Valor total:</strong> R$ {compraResponse.ValorTotal.ToString("N2")}</p>
                                <p><strong>Seus números:</strong> <span class='numbers'>{string.Join(", ", compraResponse.NumerosGerados)}</span></p>
                                <p><strong>Status do pagamento:</strong> {compraResponse.StatusPagamento}</p>
                            </div>
                            
                            <p>Para finalizar sua compra, realize o pagamento via PIX usando o QR Code abaixo:</p>
                            
                            <div class='qr-code'>
                                <img src='data:image/png;base64,{compraResponse.QrCodePix}' alt='QR Code PIX' width='200'>
                                <p>Ou copie o código PIX:</p>
                                <p style='font-size: 12px; word-break: break-all; background: #eee; padding: 10px;'>{compraResponse.CodigoPix}</p>
                            </div>
                            
                            <p><strong>Importante:</strong> O pagamento expira em {(compraResponse.ExpiracaoPix?.ToString("dd/MM/yyyy HH:mm") ?? "30 minutos")}. Após o pagamento, seus números serão confirmados automaticamente.</p>
                            
                            <p>Agradecemos sua participação e boa sorte!</p>
                        </div>
                        <div class='footer'>
                            <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await SendEmailAsync(compraResponse.Cliente.Email, subject, body);
                _logger.LogInformation($"E-mail de confirmação de compra enviado para {compraResponse.Cliente.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail de confirmação de compra para {compraResponse.Cliente?.Email}");
            }
        }

        public async Task SendPaymentConfirmationAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa, List<int> ticketNumbers)
        {
            if (string.IsNullOrEmpty(cliente?.Email))
            {
                _logger.LogWarning("E-mail do cliente não fornecido, notificação não será enviada");
                return;
            }

            try
            {
                string subject = $"Pagamento Confirmado - {rifa.Name}";

                // Corpo do e-mail em HTML
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #888; }}
                        .info-box {{ background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }}
                        .success-icon {{ text-align: center; margin: 20px 0; font-size: 48px; }}
                        .numbers {{ font-weight: bold; color: #4CAF50; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Pagamento Confirmado!</h1>
                        </div>
                        <div class='content'>
                            <div class='success-icon'>✅</div>
                            
                            <p>Olá <strong>{cliente.Name}</strong>,</p>
                            
                            <p>Seu pagamento para a rifa <strong>{rifa.Name}</strong> foi confirmado com sucesso!</p>
                            
                            <div class='info-box'>
                                <h3>Detalhes da compra:</h3>
                                <p><strong>Valor total:</strong> R$ {payment.Amount.ToString("N2")}</p>
                                <p><strong>Seus números:</strong> <span class='numbers'>{string.Join(", ", ticketNumbers)}</span></p>
                            </div>
                            
                            <p>O sorteio está previsto para <strong>{rifa.DrawDateTime.ToString("dd/MM/yyyy HH:mm")}</strong>.</p>
                            
                            <p>Fique atento para o resultado! Entraremos em contato caso você seja o ganhador.</p>
                            
                            <p>Agradecemos sua participação e boa sorte!</p>
                        </div>
                        <div class='footer'>
                            <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await SendEmailAsync(cliente.Email, subject, body);
                _logger.LogInformation($"E-mail de confirmação de pagamento enviado para {cliente.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail de confirmação de pagamento para {cliente?.Email}");
            }
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
