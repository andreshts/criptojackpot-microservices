using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Order.Data.Context.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Models.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Models.Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.OrderGuid)
            .HasColumnName("order_guid")
            .IsRequired();

        builder.HasIndex(o => o.OrderGuid)
            .IsUnique()
            .HasDatabaseName("ix_orders_order_guid");

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.LotteryId)
            .HasColumnName("lottery_id")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(o => o.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(o => o.DeletedAt)
            .HasColumnName("deleted_at");

        // Ignore computed properties
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.TotalItems);
        builder.Ignore(o => o.IsExpired);

        // Indexes
        builder.HasIndex(o => o.UserId).HasDatabaseName("ix_orders_user_id");
        builder.HasIndex(o => o.LotteryId).HasDatabaseName("ix_orders_lottery_id");
        builder.HasIndex(o => new { o.Status, o.ExpiresAt }).HasDatabaseName("ix_orders_status_expires_at");

        // One-to-many relationship with OrderDetails
        builder.HasMany(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

