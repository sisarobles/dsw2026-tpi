using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Dsw2026Tpi.Api.Configurations;

public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, app.Configuration);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        await EnsureRoleAsync(roleManager, Roles.Administrator);
        await EnsureRoleAsync(roleManager, Roles.Patient);
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(
            new IdentityRole(roleName));

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Error creando el rol {roleName}: {errors}");
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration config)
    {
        var email = config["AdminSeed:Email"];

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "Falta configurar AdminSeed:Email.");
        }

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            var password = config["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Falta configurar AdminSeed:Password.");
            }

            if (password.Length < 8)
            {
                throw new InvalidOperationException(
                    "La contraseña del administrador debe tener al menos 8 caracteres.");
            }

            var now = DateTime.UtcNow;

            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Error creando el administrador inicial: {errors}");
            }

            Log.Information(
                "Administrador inicial creado: {Email}",
                email);
        }

        if (!await userManager.IsInRoleAsync(
                admin, Roles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin, Roles.Administrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Error asignando el rol de administrador: {errors}");
            }
        }
    }
}
