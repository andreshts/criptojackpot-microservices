using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Identity.Data.Context.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // UserGuid for external API exposure 
        builder.Property(e => e.UserGuid)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(e => e.UserGuid).IsUnique();
        
        // KeycloakId for authentication integration
        builder.Property(e => e.KeycloakId)
            .HasColumnType(ColumnTypes.Text)
            .HasMaxLength(100);
        builder.HasIndex(e => e.KeycloakId).IsUnique();
        
        // Email verification token
        builder.Property(e => e.EmailVerificationToken)
            .HasColumnType(ColumnTypes.Text)
            .HasMaxLength(50);
        builder.HasIndex(e => e.EmailVerificationToken).IsUnique();
        builder.Property(e => e.EmailVerificationTokenExpiry);
        
        builder.Property(e => e.Name).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(100);
        builder.Property(e => e.Identification).HasColumnType(ColumnTypes.Text).HasMaxLength(50);
        builder.Property(e => e.Phone).HasColumnType(ColumnTypes.Text).HasMaxLength(50);
        builder.Property(e => e.StatePlace).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(100);
        builder.Property(e => e.City).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(100);
        builder.Property(e => e.Address).HasColumnType(ColumnTypes.Text).HasMaxLength(150);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.ImagePath).HasColumnType(ColumnTypes.Text).HasMaxLength(200);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Country).WithMany(c => c.Users).HasForeignKey(e => e.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Navigation(u=>u.Role).AutoInclude();

        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}