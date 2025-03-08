namespace RifaTech.DTOs.DTOs
{
    public class PaymentDTO
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid TicketId { get; set; }
        public TicketDTO Ticket { get; set; }
        public float Amount { get; set; }
        public string Method { get; set; }
        public bool IsConfirmed { get; set; }
        public string QrCodeBase64 { get; set; }
        public string QrCode { get; set; }
        public long? PaymentId { get; set; }
        // Add these properties
        public DateTime? ExpirationTime { get; set; }
        public int Status { get; set; } // 0=Pending, 1=Confirmed, 2=Expired
    }
}