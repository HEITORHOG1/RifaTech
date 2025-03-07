using Microsoft.AspNetCore.Identity;

namespace RifaTech.API.Context
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? CPF { get; set; }
        public bool EhAdmin { get; set; }
    }
}