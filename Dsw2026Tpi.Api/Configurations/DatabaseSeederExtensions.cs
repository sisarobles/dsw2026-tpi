using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class DatabaseSeederExtensions
    {
        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager, app.Configuration);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.Administrator))
                await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));

            if (!await roleManager.RoleExistsAsync(Roles.Patient))
                await roleManager.CreateAsync(new IdentityRole(Roles.Patient));
        }

        private static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            var email = config["AdminSeed:Email"] ?? "admin@system.com";
            var password = config["AdminSeed:Password"] ?? "Admin12345";

            if (await userManager.FindByEmailAsync(email) is not null) return;

            var now = DateTime.UtcNow;
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Administrator);
                Log.Information("Admin inicial creado: {Email}", email);
            }
            else
            {
                Log.Error("Error creando admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
