
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Data.Identity;

public class AuthenticationDbContext : IdentityDbContext
{
    public AuthenticationDbContext(
        DbContextOptions<AuthenticationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("ApplicationUsers");
        });

        builder.Entity<IdentityUser>(b =>
        {
            b.ToTable("Users");
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable("Roles");
        });

        builder.Entity<IdentityUserRole<string>>(b =>
        {
            b.ToTable("UsersRoles");
        });

        builder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.ToTable("UsersClaims");
        });

        builder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.ToTable("UsersLogins");
        });

        builder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.ToTable("RolesClaims");
        });

        builder.Entity<IdentityUserToken<string>>(b =>
        {
            b.ToTable("UsersTokens");
        });

        builder.Entity<RevokedToken>(b =>
        {
            b.ToTable("RevokedTokens");

            b.HasKey(token => token.Id);

            b.Property(token => token.Jti)
                .IsRequired()
                .HasMaxLength(128);

            b.Property(token => token.ExpiresAt)
                .IsRequired();

            b.Property(token => token.RevokedAt)
                .IsRequired();

            b.HasIndex(token => token.Jti)
                .IsUnique();
        });
    }
}
