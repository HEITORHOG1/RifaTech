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
}