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

            var usersToSeed = BuildUsersToSeed(configuration);
            foreach (var seedUser in usersToSeed)
            {
                await EnsureUserAsync(userManager, seedUser);
            }
        }

        private static IReadOnlyList<SeedUser> BuildUsersToSeed(IConfiguration configuration)
        {
            var adminEmail = configuration["AdminSeed:Email"];
            var adminPassword = configuration["AdminSeed:Password"];
            var adminNome = configuration["AdminSeed:NomeCompleto"];

            var admin = string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword)
                ? new SeedUser("Administrador", "admin@restaurante.com", "Admin@123", AdminRole)
                : new SeedUser(adminNome ?? "Administrador", adminEmail, adminPassword, AdminRole);

            return
            [
                admin,
                new SeedUser("Mariana Almeida", "cliente1@restaurante.com", "Cliente@123", ClienteRole),
                new SeedUser("Rafael Costa", "cliente2@restaurante.com", "Cliente@123", ClienteRole),
                new SeedUser("Juliana Rocha", "cliente3@restaurante.com", "Cliente@123", ClienteRole)
            ];
        }

        private static async Task EnsureUserAsync(UserManager<Usuario> userManager, SeedUser seedUser)
        {
            var user = await userManager.FindByEmailAsync(seedUser.Email);

            if (user is null)
            {
                user = new Usuario
                {
                    NomeCompleto = seedUser.NomeCompleto,
                    Email = seedUser.Email,
                    UserName = seedUser.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, seedUser.Password);
                if (!createResult.Succeeded)
                    return;
            }
            else
            {
                var needsUpdate = false;

                if (user.NomeCompleto != seedUser.NomeCompleto)
                {
                    user.NomeCompleto = seedUser.NomeCompleto;
                    needsUpdate = true;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    needsUpdate = true;
                }

                if (string.IsNullOrWhiteSpace(user.UserName))
                {
                    user.UserName = seedUser.Email;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                        return;
                }
            }

            if (!await userManager.IsInRoleAsync(user, seedUser.Role))
                await userManager.AddToRoleAsync(user, seedUser.Role);
        }

        private sealed record SeedUser(
            string NomeCompleto,
            string Email,
            string Password,
            string Role);
    }
}
