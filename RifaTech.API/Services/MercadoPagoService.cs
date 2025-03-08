using MercadoPago.Client.Payment;
using MercadoPago.Config;

namespace RifaTech.API.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MercadoPagoService> _logger;
        private readonly string _accessToken;
        private bool _isInitialized = false;

        public MercadoPagoService(IConfiguration configuration, ILogger<MercadoPagoService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _accessToken = _configuration["MercadoPago:AccessToken"];

            // Validate configuration
            if (string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogError("Mercado Pago access token is not configured");
                throw new InvalidOperationException("Mercado Pago access token is not configured");
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                // Configure MercadoPago SDK
                MercadoPagoConfig.AccessToken = _accessToken;
                _isInitialized = true;
                _logger.LogInformation("Mercado Pago SDK initialized");
            }
        }

        public async Task<MercadoPago.Resource.Payment.Payment> GetPaymentStatusAsync(long paymentId)
        {
            try
            {
                EnsureInitialized();

                var client = new PaymentClient();
                var payment = await client.GetAsync(paymentId);

                _logger.LogInformation($"Retrieved payment {paymentId} with status: {payment.Status}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving payment status for payment ID {paymentId}");
                throw;
            }
        }

        public async Task<MercadoPago.Resource.Payment.Payment> CreatePaymentAsync(PaymentCreateRequest request)
        {
            try
            {
                EnsureInitialized();

                var client = new PaymentClient();
                var payment = await client.CreateAsync(request);

                _logger.LogInformation($"Created payment with ID {payment.Id} and status: {payment.Status}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                throw;
            }
        }

        // Helper method to extract and map status
        public static API.Entities.PaymentStatus MapPaymentStatus(string mpStatus)
        {
            return mpStatus?.ToLower() switch
            {
                "approved" => API.Entities.PaymentStatus.Confirmed,
                "authorized" => API.Entities.PaymentStatus.Confirmed,
                "in_process" => API.Entities.PaymentStatus.Pending,
                "in_mediation" => API.Entities.PaymentStatus.Pending,
                "pending" => API.Entities.PaymentStatus.Pending,
                "rejected" => API.Entities.PaymentStatus.Expired,
                "cancelled" => API.Entities.PaymentStatus.Expired,
                "refunded" => API.Entities.PaymentStatus.Expired,
                "charged_back" => API.Entities.PaymentStatus.Expired,
                _ => API.Entities.PaymentStatus.Pending
            };
        }
    }
}