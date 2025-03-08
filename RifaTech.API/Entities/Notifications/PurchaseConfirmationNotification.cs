namespace RifaTech.API.Entities.Notifications
{
    public class PurchaseConfirmationNotification : NotificationBase
    {
        public string ClienteName { get; set; }
        public string RifaName { get; set; }
        public Guid RifaId { get; set; }
        public decimal ValorTotal { get; set; }
        public List<int> TicketNumbers { get; set; }
        public string QrCodeBase64 { get; set; }
        public string QrCode { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string PaymentStatus { get; set; }

        public PurchaseConfirmationNotification()
        {
            NotificationType = NotificationType.PurchaseConfirmation;
            Subject = "Confirmação de Compra - RifaTech";
        }
    }
}
