using RifaTech.DTOs.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RifaTech.API.Exceptions
{
    public class PixPaymentRequest
    {
        [Required]
        public Guid RifaId { get; set; }  // Corrigido para Guid

        [Required]
        [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")]
        public int Quantidade { get; set; }

        [Required]
        [Range(0.01, 9999.99, ErrorMessage = "Valor total deve ser maior que zero.")]
        public decimal ValorTotal { get; set; }

        [Required]
        public Guid ClienteId { get; set; }  // Corrigido para Guid

        // Flag para notificação por email
        public bool EnviarEmailConfirmacao { get; set; } = true;
    }
}