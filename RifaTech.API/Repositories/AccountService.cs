using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RifaTech.API.Context;
using RifaTech.DTOs.Contracts;
using RifaTech.DTOs.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static RifaTech.DTOs.Responses.ServiceResponses;

namespace RifaTech.API.Repositories
{
    public class AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
        : IUserAccount
    {
        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            if (userDTO is null) return new GeneralResponse(false, "Model is empty");
            var newUser = new ApplicationUser()
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                UserName = userDTO.Email,
                CPF = userDTO.CPF,
                EhAdmin = userDTO.EhAdmin
            };

            var user = await userManager.FindByEmailAsync(newUser.Email);
            if (user is not null) return new GeneralResponse(false, "User registered already");

            var createUser = await userManager.CreateAsync(newUser, userDTO.Password);
            if (!createUser.Succeeded) return new GeneralResponse(false, "Error occurred.. please try again");

            // Assign role based on EhAdmin flag
            if (newUser.EhAdmin)
            {
                await userManager.AddToRoleAsync(newUser, "Admin");
                // Adicionar claims específicas para o role Admin
                var adminClaim = new Claim("AdminClaimType", "AdminClaimValue");
                await userManager.AddClaimAsync(newUser, adminClaim);
            }
            else
            {
                await userManager.AddToRoleAsync(newUser, "User");
                // Adicionar claims específicas para o role User
                var userClaim = new Claim("UserClaimType", "UserClaimValue");
                await userManager.AddClaimAsync(newUser, userClaim);
            }

            return new GeneralResponse(true, "Account Created");
        }

        public async Task<LoginResponse> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return new LoginResponse(false, null!, null!, "Login container is empty", false, null!, null!);

            var getUser = await userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
                return new LoginResponse(false, null!, null!, "User not found", false, null!, null!);

            bool checkUserPasswords = await userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPasswords)
                return new LoginResponse(false, null!, null!, "Invalid email/password", false, null!, null!);

            var getUserRole = await userManager.GetRolesAsync(getUser);
            string role = getUserRole.FirstOrDefault() ?? string.Empty; // Pega a primeira função do usuário ou uma string vazia
            bool ehAdmin = getUser.EhAdmin || role == "Admin"; // Verifica se o usuário é admin

            var userSession = new UserSession(getUser.Id, getUser.Name, getUser.Email, getUserRole.ToList());
            string token = GenerateToken(userSession);
            string refreshToken = GenerateRefreshToken();

            // Salvar o refresh token no banco de dados associado ao usuário

            return new LoginResponse(true, token, refreshToken, "Login completed", ehAdmin, role, getUser.Id);
        }

        // Método GenerateToken atualizado
        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
            };

            // Adicionar roles como claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Método GenerateRefreshToken corrigido
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<UserInfoDTO> GetUserInfo(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var userInfo = new UserInfoDTO
            {
                Email = user.Email,

                IsEmailConfirmed = Equals(user.EmailConfirmed, true),
            };

            return userInfo;
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            // Verificar se o refresh token é válido e não expirou
            var user = await FindUserByRefreshToken(refreshToken);
            if (user == null)
            {
                return new LoginResponse(false, null, null, "Invalid refresh token", false, null!, null!);
            }

            // Obter os papéis do usuário
            var userRoles = await userManager.GetRolesAsync(user);
            string role = userRoles.FirstOrDefault() ?? string.Empty; // Pega a primeira função do usuário ou uma string vazia
            bool ehAdmin = user.EhAdmin || role == "Admin"; // Verifica se o usuário é admin

            // Criar uma nova sessão de usuário com os papéis
            var userSession = new UserSession(user.Id, user.Name, user.Email, userRoles.ToList());
            string newToken = GenerateToken(userSession);

            // Aqui você deve atualizar o refresh token no banco de dados se necessário

            return new LoginResponse(true, newToken, refreshToken, "Token refreshed successfully", ehAdmin, role, user.Id);
        }

        // Método auxiliar para encontrar um usuário pelo refresh token
        private async Task<ApplicationUser> FindUserByRefreshToken(string refreshToken)
        {
            // Implemente a lógica para encontrar um usuário pelo refresh token
            return await userManager.Users.FirstOrDefaultAsync(u => u.UserName == refreshToken);
        }

        public async Task<IEnumerable<UserInfoDTO>> GetAllUsersAsync()
        {
            var users = await userManager.Users.ToListAsync();
            return users.Select(u => new UserInfoDTO
            {
                Email = u.Email,
                IsEmailConfirmed = u.EmailConfirmed,
                Name = u.Name
            });
        }
    }
}