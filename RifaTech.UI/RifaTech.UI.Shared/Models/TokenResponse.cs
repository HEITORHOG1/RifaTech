namespace RifaTech.UI.Shared.Models
{
    public class TokenResponse
    {
        public bool Flag { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }
        public bool EhAdmin { get; set; }
        public string Role { get; set; }
        public string Id { get; set; }
    }
}
