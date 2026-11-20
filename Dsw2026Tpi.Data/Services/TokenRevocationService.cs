using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Data.Services;

public class TokenRevocationService : ITokenRevocationService
{
    private readonly AuthenticationDbContext _context;

    public TokenRevocationService(AuthenticationDbContext context)
    {
        _context = context;
    }

    public async Task RevokeAsync(string jti, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            throw new ArgumentException(
                "El identificador del token es obligatorio.",
                nameof(jti));
        }

        var alreadyRevoked = await _context.RevokedTokens
            .AnyAsync(token => token.Jti == jti);

        if (alreadyRevoked)
            return;

        var revokedToken = new RevokedToken(jti, expiresAt);

        _context.RevokedTokens.Add(revokedToken);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsRevokedAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return false;

        var now = DateTime.UtcNow;

        return await _context.RevokedTokens
            .AnyAsync(token =>
                token.Jti == jti &&
                token.ExpiresAt > now);
    }
}
