using System.ComponentModel.DataAnnotations;

namespace RifaTech.API.Entities
{
    public class Draw : EntityBase
    {
        [Required(ErrorMessage = "RifaId é obrigatório.")]
        public Guid RifaId { get; set; }

        [Required(ErrorMessage = "Data e hora do sorteio são obrigatórios.")]
        public DateTime DrawDateTime { get; set; }

        [Required(ErrorMessage = "Número vencedor é obrigatório.")]
        [StringLength(50, ErrorMessage = "Número vencedor não pode exceder 50 caracteres.")]
        public string WinningNumber { get; set; }

        // Propriedade de navegação
        public Rifa Rifa { get; set; }
    }
}