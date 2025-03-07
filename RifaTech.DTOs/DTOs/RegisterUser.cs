using System.ComponentModel.DataAnnotations;

namespace RifaTech.DTOs.DTOs
{
    public record RegisterUser()
    {
        [EmailAddress, Required]
        public string? Email { get; set; }
        [DataType(DataType.Password), Required]
        public string? Password { get; set; }
    }
}