using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RifaTech.API.Entities
{
    public class ExtraNumber : EntityBase
    {
        [Required(ErrorMessage = "RifaId é obrigatório.")]
        public Guid RifaId { get; set; }

        [Required(ErrorMessage = "Número é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Número deve ser maior que 0.")]
        public int Number { get; set; }

        /// <summary>
        /// Valor do prêmio. Usar decimal para precisão monetária.
        /// </summary>
        [Required(ErrorMessage = "Valor do prêmio é obrigatório.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Valor do prêmio deve ser maior que 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrizeAmount { get; set; }

        [JsonIgnore]
        public Rifa Rifa { get; set; }
    }
}