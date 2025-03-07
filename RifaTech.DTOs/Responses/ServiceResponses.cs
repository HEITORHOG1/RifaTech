namespace RifaTech.DTOs.Responses
{
    public class ServiceResponses
    {
        public record class GeneralResponse(bool Flag, string Message);
        public record class LoginResponse(bool Flag, string Token, string RefreshToken, string Message, bool EhAdmin, string Role, string Id);
    }
}