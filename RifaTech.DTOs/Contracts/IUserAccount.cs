using RifaTech.DTOs.DTOs;
using static RifaTech.DTOs.Responses.ServiceResponses;

namespace RifaTech.DTOs.Contracts
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAccount(UserDTO userDTO);

        Task<LoginResponse> LoginAccount(LoginDTO loginDTO);

        Task<UserInfoDTO> GetUserInfo(string userId);

        Task<LoginResponse> RefreshTokenAsync(string refreshToken);

        Task<IEnumerable<UserInfoDTO>> GetAllUsersAsync();
    }
}