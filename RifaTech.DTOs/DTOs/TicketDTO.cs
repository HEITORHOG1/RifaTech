namespace RifaTech.DTOs.DTOs
{
    public class TicketDTO
    {
        public Guid Id { get; set; }
        public Guid RifaId { get; set; }
        public RifaDTO Rifa { get; set; }
        public Guid ClienteId { get; set; }
        public int Number { get; set; }
        public Guid? PaymentId { get; set; }
        // Add these properties
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? GeneratedTime { get; set; }
        public int Quantidade { get; set; } // Add this property

        public ClienteDTO Cliente { get; set; }
    }
}