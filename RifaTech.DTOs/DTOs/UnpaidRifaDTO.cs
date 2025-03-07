namespace RifaTech.DTOs.DTOs
{
    public class UnpaidRifaDTO
    {
        public Guid ClienteId { get; set; }
        public Guid RifaId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal? Quantidade { get; set; }
        public decimal? PrecoTotal { get; set; }
    }
}