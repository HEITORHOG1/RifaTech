using RifaTech.API.Entities.Notifications;
using RifaTech.DTOs.DTOs;

namespace RifaTech.API.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Envia uma notificação genérica
        /// </summary>
        Task SendNotificationAsync(NotificationBase notification);

        /// <summary>
        /// Envia uma notificação de confirmação de compra
        /// </summary>
        Task SendPurchaseConfirmationAsync(CompraRapidaResponseDTO compraResponse);

        /// <summary>
        /// Envia uma notificação de confirmação de pagamento
        /// </summary>
        Task SendPaymentConfirmationAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa, List<int> ticketNumbers);

        /// <summary>
        /// Envia uma notificação de pagamento expirado
        /// </summary>
        Task SendPaymentExpiredAsync(PaymentDTO payment, ClienteDTO cliente, RifaDTO rifa);

        /// <summary>
        /// Envia um lembrete de sorteio
        /// </summary>
        Task SendDrawReminderAsync(RifaDTO rifa, ClienteDTO cliente, List<int> ticketNumbers);

        /// <summary>
        /// Envia uma notificação de resultado de sorteio
        /// </summary>
        Task SendDrawResultAsync(RifaDTO rifa, DrawDTO draw, List<ClienteDTO> participantes);

        /// <summary>
        /// Envia uma notificação ao ganhador
        /// </summary>
        Task SendWinnerNotificationAsync(RifaDTO rifa, DrawDTO draw, ClienteDTO winner, int winningNumber);
    }
}