using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CryptoJackpot.Identity.Data.Context.Configurations;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("user_refresh_tokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(256);
        builder.Property(x => x.FamilyId).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.IsRevoked).HasDefaultValue(false);
        builder.Property(x => x.RevokedReason).HasMaxLength(100);
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(256);
        builder.Property(x => x.DeviceInfo).HasMaxLength(512);
        builder.Property(x => x.IpAddress).HasMaxLength(45); // IPv6 max length

        // ─── Índices ─────────────────────────────────────────────
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAt })
            .HasDatabaseName("ix_user_refresh_tokens_active");

        // ─── No aplicar soft delete filter aquí ──────────────────
        // Los tokens revocados se mantienen para auditoría y detección de reuso.
        // Limpiar via job programado (tokens expirados > 90 días).
    }
}