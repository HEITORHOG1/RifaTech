using System.Text.Json.Serialization;

namespace RifaTech.DTOs.DTOs
{
    public class ExtraNumberDTO
    {
        public Guid Id { get; set; }
        public Guid RifaId { get; set; }
        public int Number { get; set; }

        /// <summary>
        /// Valor do prêmio. Usar decimal para precisão monetária.
        /// </summary>
        public decimal PrizeAmount { get; set; }

        [JsonIgnore]
        public RifaDTO? Rifa { get; set; }
    }
}