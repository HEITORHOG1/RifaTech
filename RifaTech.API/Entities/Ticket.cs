namespace RifaTech.API.Entities
{
    public class Ticket : EntityBase
    {
        public Guid RifaId { get; set; }
        public Rifa? Rifa { get; set; }
        public Guid ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public int Number { get; set; }
        public bool EhValido { get; set; }
        public Guid? PaymentId { get; set; }
        public DateTime GeneratedTime { get; set; }
    }
}