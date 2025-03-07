namespace RifaTech.DTOs.DTOs
{
    public class DrawDTO
    {
        public Guid Id { get; set; }
        public Guid RifaId { get; set; }
        public DateTime DrawDateTime { get; set; }
        public string? WinningNumber { get; set; }

        // Propriedade de navegação
        public RifaDTO? Rifa { get; set; }
    }
}