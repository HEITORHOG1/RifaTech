namespace RifaTech.API.Entities.Notifications
{
    public class PaymentExpiredNotification : NotificationBase
    {
        public string ClienteName { get; set; }
        public string RifaName { get; set; }
        public Guid RifaId { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime ExpirationTime { get; set; }

        public PaymentExpiredNotification()
        {
            NotificationType = NotificationType.PaymentExpired;
            Subject = "Pagamento Expirado - RifaTech";
        }
    }
}