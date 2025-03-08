namespace RifaTech.API.Entities.Notifications
{
    public class PaymentConfirmationNotification : NotificationBase
    {
        public string ClienteName { get; set; }
        public string RifaName { get; set; }
        public Guid RifaId { get; set; }
        public decimal ValorTotal { get; set; }
        public List<int> TicketNumbers { get; set; }
        public DateTime DrawDateTime { get; set; }

        public PaymentConfirmationNotification()
        {
            NotificationType = NotificationType.PaymentConfirmation;
            Subject = "Pagamento Confirmado - RifaTech";
        }
    }
}
