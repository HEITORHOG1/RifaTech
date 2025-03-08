using MercadoPago.Client.Payment;

namespace RifaTech.API.Services
{
    public interface IMercadoPagoService
    {
        Task<MercadoPago.Resource.Payment.Payment> GetPaymentStatusAsync(long paymentId);

        Task<MercadoPago.Resource.Payment.Payment> CreatePaymentAsync(PaymentCreateRequest request);
    }
}