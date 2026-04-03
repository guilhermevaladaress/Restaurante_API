using Microsoft.AspNetCore.Identity;
using Restaurante.API.Models;

namespace Restaurante.API.Data
{
    public static class IdentityInitializer
    {
        public const string AdminRole = "Admin";
        public const string ClienteRole = "Cliente";

        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<Usuario> userManager,
            IConfiguration configuration)
        {
            if (!await roleManager.RoleExistsAsync(AdminRole))
                await roleManager.CreateAsync(new IdentityRole(AdminRole));

            if (!await roleManager.RoleExistsAsync(ClienteRole))
                await roleManager.CreateAsync(new IdentityRole(ClienteRole));

            var adminEmail = configuration["AdminSeed:Email"];
            var adminPassword = configuration["AdminSeed:Password"];
            var adminNome = configuration["AdminSeed:NomeCompleto"] ?? "Administrador";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                return;

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new Usuario
                {
                    NomeCompleto = adminNome,
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true
                };

                var create = await userManager.CreateAsync(admin, adminPassword);
                if (!create.Succeeded)
                    return;
            }

            if (!await userManager.IsInRoleAsync(admin, AdminRole))
                await userManager.AddToRoleAsync(admin, AdminRole);
        }
    }
}
