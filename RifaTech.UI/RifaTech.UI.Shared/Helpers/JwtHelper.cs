using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RifaTech.UI.Shared.Helpers
{
    public static class JwtHelper
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            if (string.IsNullOrEmpty(jwt))
            {
                return claims;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return claims;
                }

                // Extrair claims do token
                claims.AddRange(jsonToken.Claims);

                // Verificar e adicionar claims de roles especiais
                var rolesClaims = jsonToken.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).ToList();

                // Se tivermos roles no formato de um array JSON, convertemos para claims individuais
                foreach (var roleClaim in rolesClaims)
                {
                    // Se o valor da claim começar com '[' e terminar com ']', pode ser um array JSON
                    if (roleClaim.Value.StartsWith("[") && roleClaim.Value.EndsWith("]"))
                    {
                        try
                        {
                            var roles = JsonSerializer.Deserialize<string[]>(roleClaim.Value);
                            if (roles != null)
                            {
                                foreach (var role in roles)
                                {
                                    // Adicionar cada role como uma claim separada do tipo ClaimTypes.Role
                                    claims.Add(new Claim(ClaimTypes.Role, role));
                                }
                            }
                        }
                        catch
                        {
                            // Se falhar a deserialização, usamos o valor original
                            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roleClaim.Value))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                            }
                        }
                    }
                    else
                    {
                        // Se o valor não for um array JSON, usamos o valor diretamente
                        if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roleClaim.Value))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                        }
                    }
                }

                return claims;
            }
            catch
            {
                return claims;
            }
        }

        public static bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return true;
                }

                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        public static DateTime? GetTokenExpirationTime(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                return jsonToken?.ValidTo;
            }
            catch
            {
                return null;
            }
        }
    }
}
