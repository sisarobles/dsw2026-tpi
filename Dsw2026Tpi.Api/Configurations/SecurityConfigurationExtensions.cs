using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Dsw2026Tpi.Api.Configurations;

public static class SecurityConfigurationExtensions
{
    public static IServiceCollection AddAppAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("Jwt");

        var keyText = jwtConfig["Key"];
        var issuer = jwtConfig["Issuer"];
        var audience = jwtConfig["Audience"];

        if (string.IsNullOrWhiteSpace(keyText))
        {
            throw new InvalidOperationException(
                "Falta configurar Jwt:Key.");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "Falta configurar Jwt:Issuer.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "Falta configurar Jwt:Audience.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(keyText);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key debe tener al menos 32 bytes para utilizar HMAC-SHA256.");
        }

        var signingKey = new SymmetricSecurityKey(keyBytes);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        RequireSignedTokens = true,
                        ValidAlgorithms =
                        [
                            SecurityAlgorithms.HmacSha256
                        ],

                        NameClaimType = System.Security.Claims.ClaimTypes.Name,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role
                    };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jti = context.Principal?
                            .FindFirst(JwtRegisteredClaimNames.Jti)?
                            .Value;

                        if (string.IsNullOrWhiteSpace(jti))
                        {
                            context.Fail(
                                "El token no contiene un identificador válido.");

                            return;
                        }

                        var revocationService = context.HttpContext
                            .RequestServices
                            .GetRequiredService<ITokenRevocationService>();

                        var isRevoked = await revocationService.IsRevokedAsync(jti);

                        if (isRevoked)
                        {
                            context.Fail("El token fue revocado.");
                        }
                    },

                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        context.Response.ContentType = "application/json";

                        var error = new AuthenticationException().Error;

                        await context.Response.WriteAsJsonAsync(error);
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        context.Response.ContentType = "application/json";

                        var error = new AuthorizationException().Error;

                        await context.Response.WriteAsJsonAsync(error);
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                Policies.AdminPolicy,
                policy => policy.RequireRole(Roles.Administrator))
            .AddPolicy(
                Policies.PatientPolicy,
                policy => policy.RequireRole(Roles.Patient));

        return services;
    }

    public static IServiceCollection AddAppCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Debe configurar Cors:AllowedOrigins fuera del entorno de desarrollo.");
            }

            allowedOrigins =
            [
                "http://localhost",
                "https://localhost"
            ];
        }

        foreach (var origin in allowedOrigins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp
                    && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    $"El origen CORS configurado no es válido: {origin}");
            }
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddAppIdentity(
        this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password = new PasswordOptions
            {
                // Se aceptan DNI de 7 y 8 dígitos.
                RequiredLength = 7,
                RequireLowercase = false,
                RequireUppercase = false,
                RequireDigit = false,
                RequireNonAlphanumeric = false
            };

            // Bloqueo de cuentas ante intentos fallidos.
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(5);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AuthenticationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        return services;
    }
}
