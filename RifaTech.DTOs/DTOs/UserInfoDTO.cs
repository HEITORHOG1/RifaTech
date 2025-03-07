namespace RifaTech.DTOs.DTOs
{
    public class UserInfoDTO
    {
        public string? Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        // Adicione outras propriedades conforme necessário, como Nome, Sobrenome, etc.

        // Exemplo de propriedades adicionais
        public string? Name { get; set; }

        // public string LastName { get; set; }
        // public DateTime DateOfBirth { get; set; }
    }
}