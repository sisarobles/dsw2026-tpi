namespace Dsw2026Tpi.Domain.Interfaces;

public interface ITokenRevocationService
{
    Task RevokeAsync(string jti, DateTime expiresAt);

    Task<bool> IsRevokedAsync(string jti);
}
