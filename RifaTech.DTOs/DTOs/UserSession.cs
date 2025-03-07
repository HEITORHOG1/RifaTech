namespace RifaTech.DTOs.DTOs
{
    public class UserSession
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } // Adicionando a propriedade Roles

        public UserSession(string id, string name, string email, List<string> roles)
        {
            Id = id;
            Name = name;
            Email = email;
            Roles = roles;
        }
    }
}