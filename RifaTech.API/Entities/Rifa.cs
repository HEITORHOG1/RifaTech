using System.ComponentModel.DataAnnotations;

namespace RifaTech.API.Entities
{
    public class Rifa : EntityBase
    {
        [Required, StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, Range(0.1, 99999.99, ErrorMessage = "O preço do ticket deve estar entre 0.1 e 99999.99.")]
        public decimal TicketPrice { get; set; }

        [Required]
        public DateTime DrawDateTime { get; set; }

        public int? WinningNumber { get; set; }

        [Required, Range(1, 999999, ErrorMessage = "O número mínimo de tickets deve estar entre 1 e 99999.")]
        public int MinTickets { get; set; }

        [Required, Range(1, 999999, ErrorMessage = "O número máximo de tickets deve estar entre 1 e 99999.")]
        public int MaxTickets { get; set; }

        public string? Base64Img { get; set; }

        // Novos campos
        public string UserId { get; set; } // ID do usuário que criou a rifa

        public string RifaLink { get; set; } // Link da rifa

        public string UniqueId { get; set; } // Campo para o link único

        // Relacionamento com Tickets
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Relacionamento com Números Extras
        public List<ExtraNumber> ExtraNumbers { get; set; } = new List<ExtraNumber>();

        public bool? EhDeleted { get; set; }

        [Required, Range(0.1, 99999.99, ErrorMessage = "O Valor do premio deve estar entre 0.1 e 99999.99.")]
        public decimal PriceValue { get; set; }
    }
}