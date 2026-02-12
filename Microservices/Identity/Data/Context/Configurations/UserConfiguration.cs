using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Identity.Data.Context.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);

        // ─── Identidad ──────────────────────────────────────────
        builder.Property(x => x.UserGuid)
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(150);
        builder.Property(x => x.EmailVerified).HasDefaultValue(false);
        builder.Property(x => x.EmailVerificationToken).HasMaxLength(256);

        // ─── Credenciales ────────────────────────────────────────
        builder.Property(x => x.PasswordHash).HasMaxLength(256);
        builder.Property(x => x.PasswordResetToken).HasMaxLength(256);

        // ─── Login Retry / Lockout ───────────────────────────────
        builder.Property(x => x.FailedLoginAttempts).HasDefaultValue(0);

        // ─── Google OAuth ────────────────────────────────────────
        builder.Property(x => x.GoogleId).HasMaxLength(128);
        builder.Property(x => x.GoogleAccessToken).HasMaxLength(2048);
        builder.Property(x => x.GoogleRefreshToken).HasMaxLength(1024);

        // ─── 2FA ─────────────────────────────────────────────────
        builder.Property(x => x.TwoFactorEnabled).HasDefaultValue(false);
        builder.Property(x => x.TwoFactorSecret).HasMaxLength(512);

        // ─── Perfil ──────────────────────────────────────────────
        builder.Property(x => x.Identification).HasMaxLength(50);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.StatePlace).IsRequired().HasMaxLength(100);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(150);
        builder.Property(x => x.ImagePath).HasMaxLength(200);

        // ─── Índices ─────────────────────────────────────────────
        builder.HasIndex(x => x.UserGuid).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.GoogleId).IsUnique().HasFilter("google_id IS NOT NULL");

        // ─── Relaciones ──────────────────────────────────────────
        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RecoveryCodes)
            .WithOne(rc => rc.User)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Soft Delete Filter ──────────────────────────────────
        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}