using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RifaTech.API.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly bool _enabled;

        public WhatsAppService(
            IConfiguration configuration,
            ILogger<WhatsAppService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;

            // Load configuration
            _accessToken = _configuration["WhatsApp:AccessToken"];
            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
            _enabled = bool.Parse(_configuration["WhatsApp:Enabled"] ?? "false");

            // Configure HTTP client
            _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v16.0/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            if (!_enabled)
            {
                _logger.LogInformation($"WhatsApp notifications are disabled. Would have sent message to {phoneNumber}");
                return true;
            }

            try
            {
                // Format phone number (remove any non-digit characters)
                phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

                // Ensure phone number has country code
                if (!phoneNumber.StartsWith("55"))
                {
                    phoneNumber = "55" + phoneNumber;
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = phoneNumber,
                    type = "text",
                    text = new
                    {
                        preview_url = false,
                        body = message
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"WhatsApp message sent successfully to {phoneNumber}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send WhatsApp message: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending WhatsApp message to {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> SendPurchaseConfirmationAsync(string phoneNumber, string clientName, string rifaName,
            decimal totalValue, List<int> ticketNumbers, string pixCode, DateTime? expirationTime)
        {
            var message = $"🎟️ *Confirmação de Compra - RifaTech* 🎟️\n\n" +
                          $"Olá *{clientName}*,\n\n" +
                          $"Sua compra para a rifa *{rifaName}* foi registrada com sucesso!\n\n" +
                          $"*Detalhes da compra:*\n" +
                          $"Valor total: R$ {totalValue:N2}\n" +
                          $"Seus números: {string.Join(", ", ticketNumbers)}\n\n" +
                          $"Para finalizar sua compra, realize o pagamento via PIX usando o código abaixo:\n\n" +
                          $"{pixCode}\n\n";

            if (expirationTime.HasValue)
            {
                message += $"*Importante:* O pagamento expira em {expirationTime.Value:dd/MM/yyyy HH:mm}.\n\n";
            }

            message += "Após o pagamento, seus números serão confirmados automaticamente.\n\n" +
                      "Agradecemos sua participação e boa sorte! 🍀";

            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendPaymentConfirmationAsync(string phoneNumber, string clientName, string rifaName,
            decimal totalValue, List<int> ticketNumbers, DateTime drawDateTime)
        {
            var message = $"✅ *Pagamento Confirmado - RifaTech* ✅\n\n" +
                          $"Olá *{clientName}*,\n\n" +
                          $"Seu pagamento para a rifa *{rifaName}* foi confirmado com sucesso!\n\n" +
                          $"*Detalhes da compra:*\n" +
                          $"Valor total: R$ {totalValue:N2}\n" +
                          $"Seus números: {string.Join(", ", ticketNumbers)}\n" +
                          $"Data do sorteio: {drawDateTime:dd/MM/yyyy HH:mm}\n\n" +
                          $"Agradecemos sua participação e boa sorte no sorteio! 🍀\n\n" +
                          $"Você receberá um aviso próximo à data do sorteio.";

            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendDrawReminderAsync(string phoneNumber, string clientName, string rifaName,
            List<int> ticketNumbers, DateTime drawDateTime, TimeSpan timeRemaining)
        {
            string timeRemainingText;
            if (timeRemaining.TotalDays >= 1)
            {
                timeRemainingText = $"{Math.Floor(timeRemaining.TotalDays)} dias e {timeRemaining.Hours} horas";
            }
            else if (timeRemaining.TotalHours >= 1)
            {
                timeRemainingText = $"{Math.Floor(timeRemaining.TotalHours)} horas e {timeRemaining.Minutes} minutos";
            }
            else
            {
                timeRemainingText = $"{timeRemaining.Minutes} minutos";
            }

            var message = $"⏰ *Lembrete de Sorteio - RifaTech* ⏰\n\n" +
                          $"Olá *{clientName}*,\n\n" +
                          $"O sorteio da rifa *{rifaName}* está se aproximando!\n\n" +
                          $"*Tempo restante:* {timeRemainingText}\n\n" +
                          $"*Detalhes da sua participação:*\n" +
                          $"Seus números: {string.Join(", ", ticketNumbers)}\n" +
                          $"Data do sorteio: {drawDateTime:dd/MM/yyyy HH:mm}\n\n" +
                          $"Não perca! Você poderá acompanhar o resultado em nosso site.\n\n" +
                          $"Boa sorte! 🍀";

            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendDrawResultAsync(string phoneNumber, string clientName, string rifaName,
            int winningNumber, string winnerName, DateTime drawDateTime)
        {
            var message = $"🎲 *Resultado do Sorteio - RifaTech* 🎲\n\n" +
                          $"Olá *{clientName}*,\n\n" +
                          $"O sorteio da rifa *{rifaName}* foi realizado!\n\n" +
                          $"*Número sorteado:* {winningNumber}\n" +
                          $"*Ganhador:* {winnerName}\n" +
                          $"*Data do sorteio:* {drawDateTime:dd/MM/yyyy HH:mm}\n\n" +
                          $"Não foi o ganhador desta vez? Não desanime! Temos novas rifas disponíveis em nosso site.\n\n" +
                          $"Obrigado por participar! 🙏";

            return await SendMessageAsync(phoneNumber, message);
        }

        public async Task<bool> SendWinnerNotificationAsync(string phoneNumber, string clientName, string rifaName,
            int winningNumber, decimal prizeValue, string contactInfo)
        {
            var message = $"🎉 *PARABÉNS! VOCÊ GANHOU! - RifaTech* 🎉\n\n" +
                          $"Olá *{clientName}*,\n\n" +
                          $"*PARABÉNS!* Você é o grande vencedor da rifa *{rifaName}*!\n\n" +
                          $"*Prêmio:* R$ {prizeValue:N2}\n" +
                          $"*Seu número sorteado:* {winningNumber}\n\n" +
                          $"*Para receber seu prêmio:*\n{contactInfo}\n\n" +
                          $"Entre em contato o mais breve possível para combinarmos a entrega do seu prêmio!\n\n" +
                          $"🏆 Parabéns novamente! 🏆";

            return await SendMessageAsync(phoneNumber, message);
        }
    }
}