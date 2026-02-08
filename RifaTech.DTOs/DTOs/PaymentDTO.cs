namespace RifaTech.DTOs.DTOs
{
    public class PaymentDTO
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid TicketId { get; set; }
        public TicketDTO? Ticket { get; set; }

        /// <summary>
        /// Valor do pagamento. Usar decimal para precisão monetária.
        /// </summary>
        public decimal Amount { get; set; }

        public string? Method { get; set; }
        public bool IsConfirmed { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? QrCode { get; set; }
        public long? PaymentId { get; set; }

        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// Status do pagamento: 0=Pending, 1=Confirmed, 2=Expired
        /// </summary>
        public int Status { get; set; }
    }
}