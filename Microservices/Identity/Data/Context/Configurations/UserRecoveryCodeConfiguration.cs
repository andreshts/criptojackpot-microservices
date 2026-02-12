using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CryptoJackpot.Identity.Data.Context.Configurations;

public class UserRecoveryCodeConfiguration : IEntityTypeConfiguration<UserRecoveryCode>
{
    public void Configure(EntityTypeBuilder<UserRecoveryCode> builder)
    {
        builder.ToTable("user_recovery_codes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CodeHash).IsRequired().HasMaxLength(256);
        builder.Property(x => x.IsUsed).HasDefaultValue(false);

        // ─── Índices ─────────────────────────────────────────────
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsUsed })
            .HasDatabaseName("ix_user_recovery_codes_available");
    }
}