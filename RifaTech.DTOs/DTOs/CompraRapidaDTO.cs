using System.ComponentModel.DataAnnotations;

namespace RifaTech.DTOs.DTOs
{
    public class CompraRapidaDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [StringLength(60, ErrorMessage = "Nome não pode exceder 60 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email em formato inválido.")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "Telefone não pode exceder 20 caracteres.")]
        public string PhoneNumber { get; set; }

        [StringLength(14, ErrorMessage = "CPF não pode exceder 14 caracteres.")]
        public string CPF { get; set; }

        [Required(ErrorMessage = "Quantidade de tickets é obrigatória.")]
        [Range(1, 100, ErrorMessage = "Quantidade deve estar entre 1 e 100.")]
        public int Quantidade { get; set; }
    }
}
