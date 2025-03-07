using Microsoft.AspNetCore.Identity;

namespace RifaTech.API.Extensions
{
    public static class RoleInitializer
    {
        public static async Task InitializeRoles(this IServiceCollection services, IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Verifica se o papel "Admin" existe
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Verifica se o papel "User" existe
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Verifica se o papel "User" existe
                if (!await roleManager.RoleExistsAsync("Mestre"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Mestre"));
                }
            }
        }
    }
}