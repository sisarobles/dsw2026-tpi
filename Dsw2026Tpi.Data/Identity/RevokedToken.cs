
namespace Dsw2026Tpi.Data.Identity;

public class RevokedToken
{
    public Guid Id { get; private set; }

    public string Jti { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime RevokedAt { get; private set; }

    // Constructor requerido por Entity Framework.
    private RevokedToken()
    {
    }

    public RevokedToken(string jti, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException(
                "El identificador del token es obligatorio.",
                nameof(jti));

        Id = Guid.NewGuid();
        Jti = jti;
        ExpiresAt = DateTime.SpecifyKind(
            expiresAt,
            DateTimeKind.Utc);
        RevokedAt = DateTime.UtcNow;
    }
}
