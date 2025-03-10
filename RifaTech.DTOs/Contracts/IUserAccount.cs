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

        // Novos métodos
        Task<UserDTO> GetUserByIdAsync(string id);
        Task<UserDTO> UpdateUserAsync(string id, UserDTO userDTO);
        Task<bool> DeleteUserAsync(string id);
        Task<UserDTO> UpdateUserRoleAsync(string id, string role);
    }
}