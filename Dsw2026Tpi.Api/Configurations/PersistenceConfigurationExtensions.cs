using Dsw2026Tpi.Data;
using Dsw2026Tpi.Data.Extensions;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Api.Configurations;

public static class PersistenceConfigurationExtensions
{
    public static IServiceCollection AddApplicationPersistence(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<Dsw2026TpiDbContext>(options =>
        {
           options.UseSqlite(connectionString);
        });

        services.AddDbContext<AuthenticationDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            options.UseSeeding((c, t) =>
            {
                c.Seedwork<IdentityRole>("Sources\\roles.json");
            });
        });
        return services;
    }
}
