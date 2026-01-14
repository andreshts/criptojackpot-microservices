using CryptoJackpot.Order.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Order.Data.Context.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.TicketGuid);

        builder.Property(t => t.TicketGuid)
            .HasColumnName("ticket_guid")
            .IsRequired();

        builder.Property(t => t.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(t => t.LotteryId)
            .HasColumnName("lottery_id")
            .IsRequired();

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.PurchaseAmount)
            .HasColumnName("purchase_amount")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(t => t.PurchaseDate)
            .HasColumnName("purchase_date")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.TransactionId)
            .HasColumnName("transaction_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.SelectedNumbers)
            .HasColumnName("selected_numbers")
            .HasColumnType("integer[]")
            .IsRequired();

        builder.Property(t => t.Series)
            .HasColumnName("series")
            .IsRequired();

        builder.Property(t => t.LotteryNumberIds)
            .HasColumnName("lottery_number_ids")
            .HasColumnType("uuid[]");

        builder.Property(t => t.IsGift)
            .HasColumnName("is_gift")
            .IsRequired();

        builder.Property(t => t.GiftRecipientId)
            .HasColumnName("gift_recipient_id");

        builder.Property(t => t.WonPrizeIds)
            .HasColumnName("won_prize_ids")
            .HasColumnType("bigint[]");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(t => t.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(t => t.UserId).HasDatabaseName("ix_tickets_user_id");
        builder.HasIndex(t => t.LotteryId).HasDatabaseName("ix_tickets_lottery_id");
        builder.HasIndex(t => t.OrderId).HasDatabaseName("ix_tickets_order_id");
    }
}

