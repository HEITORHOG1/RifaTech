namespace RifaTech.API.Entities
{
    public class UnpaidRifa : EntityBase
    {
        public Guid ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public Guid RifaId { get; set; }
        public Rifa? Rifa { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Quantidade { get; set; }
        public decimal? PrecoTotal { get; set; }
    }
}