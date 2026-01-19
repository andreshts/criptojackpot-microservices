using CryptoJackpot.Order.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Order.Data.Context.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("order_details");

        builder.HasKey(od => od.Id);

        builder.Property(od => od.Id)
            .HasColumnName("id");

        builder.Property(od => od.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(od => od.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder.Property(od => od.Quantity)
            .HasColumnName("quantity")
            .HasDefaultValue(1)
            .IsRequired();

        // Ignore computed property
        builder.Ignore(od => od.Subtotal);

        builder.Property(od => od.Number)
            .HasColumnName("number")
            .IsRequired();

        builder.Property(od => od.Series)
            .HasColumnName("series")
            .IsRequired();

        builder.Property(od => od.LotteryNumberId)
            .HasColumnName("lottery_number_id");

        builder.Property(od => od.IsGift)
            .HasColumnName("is_gift")
            .HasDefaultValue(false);

        builder.Property(od => od.GiftRecipientId)
            .HasColumnName("gift_recipient_id");

        builder.Property(od => od.TicketId)
            .HasColumnName("ticket_id");

        builder.Property(od => od.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(od => od.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(od => od.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(od => od.OrderId).HasDatabaseName("ix_order_details_order_id");
        builder.HasIndex(od => new { od.Number, od.Series }).HasDatabaseName("ix_order_details_number_series");

        // One-to-one relationship with Ticket
        builder.HasOne(od => od.Ticket)
            .WithOne(t => t.OrderDetail)
            .HasForeignKey<Ticket>(t => t.OrderDetailId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
