namespace RifaTech.API.Services
{
    /// <summary>
    /// Interface for WhatsApp notification services
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>
        /// Send a WhatsApp message to a phone number
        /// </summary>
        Task<bool> SendMessageAsync(string phoneNumber, string message);

        /// <summary>
        /// Send a purchase confirmation with ticket details
        /// </summary>
        Task<bool> SendPurchaseConfirmationAsync(string phoneNumber, string clientName, string rifaName,
            decimal totalValue, List<int> ticketNumbers, string pixCode, DateTime? expirationTime);

        /// <summary>
        /// Send a payment confirmation notification
        /// </summary>
        Task<bool> SendPaymentConfirmationAsync(string phoneNumber, string clientName, string rifaName,
            float totalValue, List<int> ticketNumbers, DateTime drawDateTime);

        /// <summary>
        /// Send a draw reminder notification
        /// </summary>
        Task<bool> SendDrawReminderAsync(string phoneNumber, string clientName, string rifaName,
            List<int> ticketNumbers, DateTime drawDateTime, TimeSpan timeRemaining);

        /// <summary>
        /// Send a draw result notification
        /// </summary>
        Task<bool> SendDrawResultAsync(string phoneNumber, string clientName, string rifaName,
            int winningNumber, string winnerName, DateTime drawDateTime);

        /// <summary>
        /// Send a winner notification
        /// </summary>
        Task<bool> SendWinnerNotificationAsync(string phoneNumber, string clientName, string rifaName,
            int winningNumber, decimal prizeValue, string contactInfo);
    }
}