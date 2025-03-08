using System.ComponentModel.DataAnnotations;

namespace RifaTech.API.Entities
{
    public class Cliente : EntityBase
    {
        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "Telefone não pode exceder 20 caracteres.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email é obrigatório.")]
        [StringLength(50, ErrorMessage = "Email não pode exceder 50 caracteres.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Name é obrigatório.")]
        [StringLength(60, ErrorMessage = "Name não pode exceder 60 caracteres.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "CPF é obrigatório.")]
        [StringLength(11, ErrorMessage = "CPF não pode exceder 11 caracteres.")]
        public string? CPF { get; set; }
    }
}