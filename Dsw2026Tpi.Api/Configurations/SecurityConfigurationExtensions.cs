using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Dsw2026Tpi.Api.Configurations;

public static class SecurityConfigurationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key");
        var issuer = jwtConfig["Issuer"] ?? throw new ArgumentNullException("JWT Issuer");
        var audience = jwtConfig["Audience"] ?? throw new ArgumentNullException("JWT Audience");
        var key = Encoding.UTF8.GetBytes(keyText);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminPolicy, policy =>
                policy.RequireRole(Roles.Administrator))
            .AddPolicy(Policies.PatientPolicy, policy =>
                policy.RequireRole(Roles.Patient));
        return services;
    }

    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>()?
                            .Where(origin => !string.IsNullOrWhiteSpace(origin))
                            .Select(origin => origin.TrimEnd('/'))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            allowedOrigins =
            [
                "http://localhost",
                "https://localhost"
            ];
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.SetIsOriginAllowed(_ => true)
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowCredentials();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowCredentials();
                }
            });
        });

        return services;
    }
    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireLowercase = false,
                RequireUppercase = false,
                RequireDigit = false,
                RequireNonAlphanumeric = false
            };

        }).AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<AuthenticationDbContext>()
          .AddSignInManager()
          .AddDefaultTokenProviders();
        return services;
    }
}
