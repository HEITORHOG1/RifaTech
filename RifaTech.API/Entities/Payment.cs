namespace RifaTech.API.Entities
{
    public class Payment : EntityBase
    {
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public float Amount { get; set; }
        public string Method { get; set; }
        public bool IsConfirmed { get; set; }
        public string QrCodeBase64 { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string QrCode { get; set; }
        public long? PaymentId { get; set; }
    }
}