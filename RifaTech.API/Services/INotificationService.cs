using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Services
{
    public interface INotificationService
    {
        Task SendPurchaseConfirmationAsync(CompraRapidaResponseDTO compraResponse);

        Task SendPaymentConfirmationAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa, List<int> ticketNumbers);
    }
}