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
                Name = user.Name
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

        // MÉTODOS NOVOS

        // 1. Obter um usuário pelo ID
        public async Task<UserDTO> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return null;
                }

                var roles = await userManager.GetRolesAsync(user);

                return new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CPF = user.CPF,
                    EhAdmin = user.EhAdmin || roles.Contains("Admin"),
                    Ativo = user.EmailConfirmed, // Presumindo que um usuário ativo tem o email confirmado
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // 2. Atualizar um usuário existente
        public async Task<UserDTO> UpdateUserAsync(string id, UserDTO userDTO)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return null;
                }

                // Atualizar propriedades
                user.Name = userDTO.Name;
                user.Email = userDTO.Email;
                user.UserName = userDTO.Email; // Assumindo que o username é igual ao email
                user.CPF = userDTO.CPF;
                user.EhAdmin = userDTO.EhAdmin;
                user.EmailConfirmed = userDTO.Ativo; // Atualizando o status ativo

                // Atualizar senha se fornecida
                if (!string.IsNullOrEmpty(userDTO.Password))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, userDTO.Password);
                    if (!result.Succeeded)
                    {
                        return null;
                    }
                }

                // Salvar alterações
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return null;
                }

                // Atualizar papéis/funções, se EhAdmin mudou
                var currentRoles = await userManager.GetRolesAsync(user);
                if (userDTO.EhAdmin && !currentRoles.Contains("Admin"))
                {
                    // Remover papel "User" se existir
                    if (currentRoles.Contains("User"))
                    {
                        await userManager.RemoveFromRoleAsync(user, "User");
                    }
                    // Adicionar papel "Admin"
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else if (!userDTO.EhAdmin && currentRoles.Contains("Admin"))
                {
                    // Remover papel "Admin"
                    await userManager.RemoveFromRoleAsync(user, "Admin");
                    // Adicionar papel "User"
                    await userManager.AddToRoleAsync(user, "User");
                }

                // Retornar o usuário atualizado
                return await GetUserByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // 3. Excluir um usuário
        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                // Remover o usuário
                var result = await userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // 4. Atualizar o papel/função de um usuário
        public async Task<UserDTO> UpdateUserRoleAsync(string id, string role)
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return null;
                }

                // Verificar se o papel/função existe
                var roleExists = await roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    return null;
                }

                // Remover todos os papéis existentes
                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);

                // Adicionar o novo papel
                await userManager.AddToRoleAsync(user, role);

                // Atualizar o campo EhAdmin com base no papel
                user.EhAdmin = role == "Admin";
                await userManager.UpdateAsync(user);

                // Retornar o usuário atualizado
                return await GetUserByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}