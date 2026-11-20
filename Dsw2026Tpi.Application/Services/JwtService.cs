using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dsw2026Tpi.Application.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userId, string username, string? role)
    {
        var jwtConfig = _config.GetSection("Jwt");

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

        if (!int.TryParse(jwtConfig["ExpiresInMinutes"], out var expiresIn)
            || expiresIn <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:ExpiresInMinutes debe ser un número entero mayor que cero.");
        }

        var key = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username)
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
