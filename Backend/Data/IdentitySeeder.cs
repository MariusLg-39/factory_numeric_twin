using Microsoft.AspNetCore.Identity;
using ProjetFullstack.Models;

namespace ProjetFullstack.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        // ============================================================
        // CRÉATION DES RÔLES
        // ============================================================

        string[] roles =
        {
            "User",
            "Admin"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role)
                );

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Erreur lors de la création du rôle '{role}': " +
                        string.Join(
                            ", ",
                            result.Errors.Select(e => e.Description)
                        )
                    );
                }
            }
        }

        // ============================================================
        // CRÉATION DE L'ADMIN INITIAL
        // ============================================================

        const string adminEmail = "admin@factory-twin.com";
        const string adminPassword = "Admin123456789!";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,

                FirstName = "Admin",
                LastName = "System",

                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(
                admin,
                adminPassword
            );

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Erreur lors de la création de l'Admin : " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    )
                );
            }
        }

        // ============================================================
        // ATTRIBUTION DU RÔLE ADMIN
        // ============================================================

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin,
                "Admin"
            );

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Erreur lors de l'attribution du rôle Admin : " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    )
                );
            }
        }
    }
}
