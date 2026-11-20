using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Data.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests;

public class TokenRevocationServiceTests
{
    private static async Task<(
        AuthenticationDbContext Context,
        TokenRevocationService Service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AuthenticationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var context = new AuthenticationDbContext(options);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var service = new TokenRevocationService(context);

        return (context, service);
    }

    [Fact]
    public async Task RevokeAsync_CuandoTokenEsValido_RegistraTokenRevocado()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            var jti = Guid.NewGuid().ToString();
            var expiration = DateTime.UtcNow.AddMinutes(30);

            await service.RevokeAsync(jti, expiration);

            var revokedToken = await context.RevokedTokens
                .SingleOrDefaultAsync(token => token.Jti == jti);

            Assert.NotNull(revokedToken);
            Assert.Equal(jti, revokedToken.Jti);
            Assert.Equal(expiration, revokedToken.ExpiresAt);
        }
    }

    [Fact]
    public async Task IsRevokedAsync_CuandoTokenFueRevocado_RetornaTrue()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            var jti = Guid.NewGuid().ToString();

            await service.RevokeAsync(
                jti,
                DateTime.UtcNow.AddMinutes(30));

            var result = await service.IsRevokedAsync(jti);

            Assert.True(result);
        }
    }

    [Fact]
    public async Task IsRevokedAsync_CuandoTokenNoFueRevocado_RetornaFalse()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            var result = await service.IsRevokedAsync(
                Guid.NewGuid().ToString());

            Assert.False(result);
        }
    }

    [Fact]
    public async Task IsRevokedAsync_CuandoTokenEstaVencido_RetornaFalse()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            var jti = Guid.NewGuid().ToString();

            await service.RevokeAsync(
                jti,
                DateTime.UtcNow.AddMinutes(-5));

            var result = await service.IsRevokedAsync(jti);

            Assert.False(result);
        }
    }

    [Fact]
    public async Task RevokeAsync_CuandoTokenYaFueRevocado_NoDuplicaRegistro()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            var jti = Guid.NewGuid().ToString();
            var expiration = DateTime.UtcNow.AddMinutes(30);

            await service.RevokeAsync(jti, expiration);
            await service.RevokeAsync(jti, expiration);

            var count = await context.RevokedTokens
                .CountAsync(token => token.Jti == jti);

            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task RevokeAsync_CuandoJtiEstaVacio_LanzaExcepcion()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.RevokeAsync(
                    " ",
                    DateTime.UtcNow.AddMinutes(30)));
        }
    }

    [Fact]
    public async Task IsRevokedAsync_CuandoJtiEstaVacio_RetornaFalse()
    {
        var (context, service) = await CreateContextAsync();

        await using (context)
        {
            var result = await service.IsRevokedAsync(" ");

            Assert.False(result);
        }
    }
}
