using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Order.Data.Context.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Models.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Models.Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.OrderGuid);

        builder.Property(o => o.OrderGuid)
            .HasColumnName("order_guid")
            .IsRequired();

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.LotteryId)
            .HasColumnName("lottery_id")
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(o => o.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(o => o.SelectedNumbers)
            .HasColumnName("selected_numbers")
            .HasColumnType("integer[]")
            .IsRequired();

        builder.Property(o => o.Series)
            .HasColumnName("series")
            .IsRequired();

        builder.Property(o => o.LotteryNumberIds)
            .HasColumnName("lottery_number_ids")
            .HasColumnType("uuid[]");

        builder.Property(o => o.IsGift)
            .HasColumnName("is_gift")
            .IsRequired();

        builder.Property(o => o.GiftRecipientId)
            .HasColumnName("gift_recipient_id");

        builder.Property(o => o.TicketId)
            .HasColumnName("ticket_id");

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(o => o.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(o => o.UserId).HasDatabaseName("ix_orders_user_id");
        builder.HasIndex(o => o.LotteryId).HasDatabaseName("ix_orders_lottery_id");
        builder.HasIndex(o => new { o.Status, o.ExpiresAt }).HasDatabaseName("ix_orders_status_expires_at");

        // Relationship
        builder.HasOne(o => o.Ticket)
            .WithOne(t => t.Order)
            .HasForeignKey<Domain.Models.Order>(o => o.TicketId);
    }
}

