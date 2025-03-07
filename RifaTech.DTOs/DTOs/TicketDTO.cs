namespace RifaTech.DTOs.DTOs
{
    public class TicketDTO
    {
        public Guid Id { get; set; }
        public Guid RifaId { get; set; }
        public required RifaDTO Rifa { get; set; }
        public Guid ClienteId { get; set; }
        public int Number { get; set; }

        public Guid? PagamentoId { get; set; }
        public string? QrCodeBase64 { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }

        public List<TicketDTO> Tickets { get; set; } = new();

        public required ClienteDTO Cliente { get; set; }
    }
}