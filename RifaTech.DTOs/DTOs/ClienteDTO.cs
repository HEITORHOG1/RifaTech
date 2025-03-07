namespace RifaTech.DTOs.DTOs
{
    public class ClienteDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? CPF { get; set; }
    }
}