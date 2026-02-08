using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RifaTech.API.Entities
{
    public class Payment : EntityBase
    {
        [Required]
        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        /// <summary>
        /// Valor do pagamento. Usar decimal para precisão monetária.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string Method { get; set; } = string.Empty;

        public bool IsConfirmed { get; set; }

        public string? QrCodeBase64 { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime ExpirationTime { get; set; }

        public string? QrCode { get; set; }

        /// <summary>
        /// ID externo do pagamento no Mercado Pago.
        /// </summary>
        public long? PaymentId { get; set; }
    }
}