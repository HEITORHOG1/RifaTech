using RifaTech.API.Entities.Notifications;
using System.Reflection;

namespace RifaTech.API.Services
{
    public class TemplateEngine : ITemplateEngine
    {
        private readonly ILogger<TemplateEngine> _logger;
        private Dictionary<string, string> _templateCache = new Dictionary<string, string>();

        public TemplateEngine(ILogger<TemplateEngine> logger)
        {
            _logger = logger;
        }

        public string RenderTemplate(string templateName, NotificationBase model)
        {
            try
            {
                // Get the template content
                string template = GetTemplateContent(templateName);

                // Replace placeholders based on model type
                return ReplacePlaceholders(template, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rendering template {templateName}");
                return GetFallbackTemplate(model);
            }
        }

        private string GetTemplateContent(string templateName)
        {
            // Check if the template is already cached
            if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
            {
                return cachedTemplate;
            }

            // Try to load the template from embedded resources
            string resourceName = $"RifaTech.API.Templates.{templateName}.html";
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                string template = reader.ReadToEnd();
                _templateCache[templateName] = template;
                return template;
            }

            // If not found in embedded resources, use hardcoded templates
            var hardcodedTemplate = GetHardcodedTemplate(templateName);
            _templateCache[templateName] = hardcodedTemplate;
            return hardcodedTemplate;
        }

        private string ReplacePlaceholders(string template, NotificationBase model)
        {
            // Replace common placeholders
            template = template.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

            // Replace specific placeholders based on model type
            switch (model)
            {
                case PurchaseConfirmationNotification purchaseConfirmation:
                    template = template.Replace("{{ClienteName}}", purchaseConfirmation.ClienteName)
                                      .Replace("{{RifaName}}", purchaseConfirmation.RifaName)
                                      .Replace("{{ValorTotal}}", FormatCurrency(purchaseConfirmation.ValorTotal))
                                      .Replace("{{TicketNumbers}}", string.Join(", ", purchaseConfirmation.TicketNumbers))
                                      .Replace("{{QrCodeBase64}}", purchaseConfirmation.QrCodeBase64)
                                      .Replace("{{QrCode}}", purchaseConfirmation.QrCode)
                                      .Replace("{{ExpirationTime}}", FormatDateTime(purchaseConfirmation.ExpirationTime))
                                      .Replace("{{StatusPagamento}}", purchaseConfirmation.PaymentStatus);
                    break;

                case PaymentConfirmationNotification paymentConfirmation:
                    template = template.Replace("{{ClienteName}}", paymentConfirmation.ClienteName)
                                      .Replace("{{RifaName}}", paymentConfirmation.RifaName)
                                      .Replace("{{ValorTotal}}", FormatCurrency(paymentConfirmation.ValorTotal))
                                      .Replace("{{TicketNumbers}}", string.Join(", ", paymentConfirmation.TicketNumbers))
                                      .Replace("{{DrawDateTime}}", FormatDateTime(paymentConfirmation.DrawDateTime));
                    break;
                // Other notification types...
                case PaymentExpiredNotification paymentExpired:
                    template = template.Replace("{{ClienteName}}", paymentExpired.ClienteName)
                                       .Replace("{{RifaName}}", paymentExpired.RifaName)
                                       .Replace("{{ValorTotal}}", FormatCurrency(paymentExpired.ValorTotal))
                                       .Replace("{{ExpirationTime}}", FormatDateTime(paymentExpired.ExpirationTime));
                    break;

                case DrawReminderNotification drawReminder:
                    template = template.Replace("{{ClienteName}}", drawReminder.ClienteName)
                                       .Replace("{{RifaName}}", drawReminder.RifaName)
                                       .Replace("{{TicketNumbers}}", string.Join(", ", drawReminder.TicketNumbers))
                                       .Replace("{{DrawDateTime}}", FormatDateTime(drawReminder.DrawDateTime))
                                       .Replace("{{TimeRemaining}}", FormatTimeRemaining(drawReminder.TimeRemaining));
                    break;

                case DrawResultNotification drawResult:
                    template = template.Replace("{{RifaName}}", drawResult.RifaName)
                                       .Replace("{{DrawDateTime}}", FormatDateTime(drawResult.DrawDateTime))
                                       .Replace("{{WinningNumber}}", drawResult.WinningNumber.ToString())
                                       .Replace("{{WinnerName}}", drawResult.WinnerName);
                    break;

                case WinnerNotification winnerNotification:
                    template = template.Replace("{{ClienteName}}", winnerNotification.ClienteName)
                                       .Replace("{{RifaName}}", winnerNotification.RifaName)
                                       .Replace("{{WinningNumber}}", winnerNotification.WinningNumber.ToString())
                                       .Replace("{{PrizeValue}}", FormatCurrency(winnerNotification.PrizeValue))
                                       .Replace("{{ContactInfo}}", winnerNotification.ContactInfo);
                    break;
            }

            return template;
        }

        private string GetHardcodedTemplate(string templateName)
        {
            // Fallback hardcoded templates
            return templateName switch
            {
                "PurchaseConfirmation" => GetPurchaseConfirmationTemplate(),
                "PaymentConfirmation" => GetPaymentConfirmationTemplate(),
                "PaymentExpired" => GetPaymentExpiredTemplate(),
                "DrawReminder" => GetDrawReminderTemplate(),
                "DrawResult" => GetDrawResultTemplate(),
                "WinnerNotification" => GetWinnerNotificationTemplate(),
                _ => GetGenericTemplate()
            };
        }

        private string GetFallbackTemplate(NotificationBase model)
        {
            // Provide a fallback template for each notification type
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                    </style>
                </head>
                <body>
                    <p>Prezado cliente,</p>
                    <p>Esta é uma notificação do RifaTech sobre seu {model.NotificationType}.</p>
                    <p>Por favor, entre em contato conosco para mais informações.</p>
                    <p>Atenciosamente,</p>
                    <p>Equipe RifaTech</p>
                </body>
                </html>";
        }

        private string FormatCurrency(decimal value)
        {
            return value.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        }

        private string FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
        }

        private string FormatTimeRemaining(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 1)
                return $"{Math.Floor(timeSpan.TotalDays)} dias e {timeSpan.Hours} horas";
            else if (timeSpan.TotalHours > 1)
                return $"{Math.Floor(timeSpan.TotalHours)} horas e {timeSpan.Minutes} minutos";
            else
                return $"{timeSpan.Minutes} minutos";
        }

        private string GetPurchaseConfirmationTemplate()
        {
            return @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; line-height: 1.6; }
                        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                        .header { background-color: #4A6FE8; color: white; padding: 10px 20px; text-align: center; }
                        .content { padding: 20px; }
                        .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
                        .info-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }
                        .qr-code { text-align: center; margin: 20px 0; }
                        .cta-button { display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px;
                                     text-decoration: none; border-radius: 5px; margin-top: 20px; }
                        .numbers { font-weight: bold; color: #4A6FE8; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Confirmação de Compra</h1>
                        </div>
                        <div class='content'>
                            <p>Olá <strong>{{ClienteName}}</strong>,</p>

                            <p>Sua compra para a rifa <strong>{{RifaName}}</strong> foi registrada com sucesso!</p>

                            <div class='info-box'>
                                <h3>Detalhes da compra:</h3>
                                <p><strong>Valor total:</strong> {{ValorTotal}}</p>
                                <p><strong>Seus números:</strong> <span class='numbers'>{{TicketNumbers}}</span></p>
                                <p><strong>Status do pagamento:</strong> {{StatusPagamento}}</p>
                            </div>

                            <p>Para finalizar sua compra, realize o pagamento via PIX usando o QR Code abaixo:</p>

                            <div class='qr-code'>
                                <img src='data:image/png;base64,{{QrCodeBase64}}' alt='QR Code PIX' width='200'>
                                <p>Ou copie o código PIX:</p>
                                <p style='font-size: 12px; word-break: break-all; background: #eee; padding: 10px;'>{{QrCode}}</p>
                            </div>

                            <p><strong>Importante:</strong> O pagamento expira em {{ExpirationTime}}. Após o pagamento, seus números serão confirmados automaticamente.</p>

                            <p>Agradecemos sua participação e boa sorte!</p>
                        </div>
                        <div class='footer'>
                            <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                            <p>&copy; {{
                                CurrentYear
                            }} RifaTech. Todos os direitos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetPaymentConfirmationTemplate()
        {
            return @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background-color: #4A6FE8; color: white; padding: 10px 20px; text-align: center; }
                .content { padding: 20px; }
                .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
                .info-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }
                .cta-button { display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px;
                             text-decoration: none; border-radius: 5px; margin-top: 20px; }
                .numbers { font-weight: bold; color: #4A6FE8; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Pagamento Confirmado</h1>
                </div>
                <div class='content'>
                    <p>Olá <strong>{{ClienteName}}</strong>,</p>

                    <p>Seu pagamento para a rifa <strong>{{RifaName}}</strong> foi confirmado com sucesso!</p>

                    <div class='info-box'>
                        <h3>Detalhes da compra:</h3>
                        <p><strong>Valor total:</strong> {{ValorTotal}}</p>
                        <p><strong>Seus números:</strong> <span class='numbers'>{{TicketNumbers}}</span></p>
                        <p><strong>Data do sorteio:</strong> {{DrawDateTime}}</p>
                    </div>

                    <p>Agradecemos sua participação e boa sorte no sorteio!</p>

                    <p>Você pode acompanhar o resultado do sorteio através do nosso site ou aplicativo.</p>

                    <a href='https://rifatech.com/sorteios' class='cta-button'>Acompanhar Sorteios</a>
                </div>
                <div class='footer'>
                    <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                    <p>&copy; {{CurrentYear}} RifaTech. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }

        private string GetPaymentExpiredTemplate()
        {
            return @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background-color: #E74C3C; color: white; padding: 10px 20px; text-align: center; }
                .content { padding: 20px; }
                .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
                .info-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }
                .cta-button { display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px;
                             text-decoration: none; border-radius: 5px; margin-top: 20px; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Pagamento Expirado</h1>
                </div>
                <div class='content'>
                    <p>Olá <strong>{{ClienteName}}</strong>,</p>

                    <p>Infelizmente, o tempo para pagamento da sua rifa <strong>{{RifaName}}</strong> expirou.</p>

                    <div class='info-box'>
                        <h3>Detalhes da compra:</h3>
                        <p><strong>Valor total:</strong> {{ValorTotal}}</p>
                        <p><strong>Data de expiração:</strong> {{ExpirationTime}}</p>
                    </div>

                    <p>Você ainda pode participar realizando uma nova compra em nosso site.</p>

                    <a href='https://rifatech.com/rifas' class='cta-button'>Comprar Novos Tickets</a>
                </div>
                <div class='footer'>
                    <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                    <p>&copy; {{CurrentYear}} RifaTech. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }

        private string GetDrawReminderTemplate()
        {
            return @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background-color: #F39C12; color: white; padding: 10px 20px; text-align: center; }
                .content { padding: 20px; }
                .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
                .info-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }
                .cta-button { display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px;
                             text-decoration: none; border-radius: 5px; margin-top: 20px; }
                .numbers { font-weight: bold; color: #4A6FE8; }
                .countdown { font-size: 20px; font-weight: bold; color: #F39C12; text-align: center; margin: 20px 0; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Lembrete de Sorteio</h1>
                </div>
                <div class='content'>
                    <p>Olá <strong>{{ClienteName}}</strong>,</p>

                    <p>O sorteio da rifa <strong>{{RifaName}}</strong> está se aproximando!</p>

                    <div class='countdown'>
                        Tempo restante: {{TimeRemaining}}
                    </div>

                    <div class='info-box'>
                        <h3>Detalhes da sua participação:</h3>
                        <p><strong>Seus números:</strong> <span class='numbers'>{{TicketNumbers}}</span></p>
                        <p><strong>Data do sorteio:</strong> {{DrawDateTime}}</p>
                    </div>

                    <p>Não perca! Você poderá acompanhar o sorteio ao vivo em nosso site.</p>

                    <a href='https://rifatech.com/sorteios/ao-vivo' class='cta-button'>Acompanhar Sorteio</a>
                </div>
                <div class='footer'>
                    <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                    <p>&copy; {{CurrentYear}} RifaTech. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }

        private string GetDrawResultTemplate()
        {
            return @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background-color: #2ECC71; color: white; padding: 10px 20px; text-align: center; }
                .content { padding: 20px; }
                .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
                .info-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }
                .cta-button { display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px;
                             text-decoration: none; border-radius: 5px; margin-top: 20px; }
                .winner { font-size: 24px; font-weight: bold; color: #2ECC71; text-align: center; margin: 20px 0; }
                .winner-number { font-size: 32px; background-color: #2ECC71; color: white; padding: 10px;
                                border-radius: 50%; display: inline-block; width: 50px; height: 50px;
                                line-height: 50px; text-align: center; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Resultado do Sorteio</h1>
                </div>
                <div class='content'>
                    <p>O sorteio da rifa <strong>{{RifaName}}</strong> foi realizado!</p>

                    <div class='winner'>
                        Número sorteado: <div class='winner-number'>{{WinningNumber}}</div>
                    </div>

                    <div class='info-box'>
                        <h3>Detalhes do sorteio:</h3>
                        <p><strong>Data do sorteio:</strong> {{DrawDateTime}}</p>
                        <p><strong>Ganhador:</strong> {{WinnerName}}</p>
                    </div>

                    <p>Não foi o ganhador desta vez? Não desanime! Temos novas rifas disponíveis.</p>

                    <a href='https://rifatech.com/rifas' class='cta-button'>Ver Novas Rifas</a>
                </div>
                <div class='footer'>
                    <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                    <p>&copy; {{CurrentYear}} RifaTech. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }

        private string GetWinnerNotificationTemplate()
        {
            return @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background-color: #2ECC71; color: white; padding: 10px 20px; text-align: center; }
                .content { padding: 20px; }
                .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
                .info-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin: 15px 0; }
                .cta-button { display: inline-block; background-color: #4A6FE8; color: white; padding: 10px 20px;
                             text-decoration: none; border-radius: 5px; margin-top: 20px; }
                .congratulations { font-size: 24px; font-weight: bold; color: #2ECC71; text-align: center; margin: 20px 0; }
                .prize { font-size: 32px; font-weight: bold; text-align: center; margin: 20px 0; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Parabéns! Você Ganhou!</h1>
                </div>
                <div class='content'>
                    <p>Olá <strong>{{ClienteName}}</strong>,</p>

                    <div class='congratulations'>
                        PARABÉNS! Você é o grande vencedor da rifa {{RifaName}}!
                    </div>

                    <div class='prize'>
                        Prêmio: {{PrizeValue}}
                    </div>

                    <div class='info-box'>
                        <h3>Detalhes do sorteio:</h3>
                        <p><strong>Seu número sorteado:</strong> {{WinningNumber}}</p>
                        <p><strong>Para receber seu prêmio:</strong> {{ContactInfo}}</p>
                    </div>

                    <p>Entre em contato o mais breve possível para combinarmos a entrega do seu prêmio!</p>
                </div>
                <div class='footer'>
                    <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                    <p>&copy; {{CurrentYear}} RifaTech. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }

        private string GetGenericTemplate()
        {
            return @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; line-height: 1.6; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background-color: #4A6FE8; color: white; padding: 10px 20px; text-align: center; }
                .content { padding: 20px; }
                .footer { font-size: 12px; text-align: center; margin-top: 20px; color: #888; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>RifaTech - Notificação</h1>
                </div>
                <div class='content'>
                    <p>Olá,</p>
                    <p>Esta é uma notificação do sistema RifaTech.</p>
                    <p>Para mais informações, acesse nosso site ou entre em contato conosco.</p>
                </div>
                <div class='footer'>
                    <p>Esta é uma mensagem automática. Por favor, não responda a este e-mail.</p>
                    <p>&copy; {{CurrentYear}} RifaTech. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }
    }
}