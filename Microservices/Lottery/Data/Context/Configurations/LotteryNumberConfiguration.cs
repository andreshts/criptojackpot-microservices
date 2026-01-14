using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Lottery.Data.Context.Configurations;

public class LotteryNumberConfiguration : IEntityTypeConfiguration<LotteryNumber>
{
    public void Configure(EntityTypeBuilder<LotteryNumber> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.LotteryId).IsRequired();
        builder.Property(e => e.Number).IsRequired();
        builder.Property(e => e.Series).IsRequired();
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(e => e.OrderId);
        builder.Property(e => e.TicketId);
        builder.Property(e => e.ReservationExpiresAt);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        
        // Ignore computed property
        builder.Ignore(e => e.IsAvailable);
        
        // Indexes
        builder.HasIndex(e => new { e.LotteryId, e.Number, e.Series })
            .HasDatabaseName("IX_LotteryNumbers_LotteryId_Number_Series").IsUnique();
        
        builder.HasIndex(e => new { e.LotteryId, e.Status })
            .HasDatabaseName("IX_LotteryNumbers_LotteryId_Status");
        
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("IX_LotteryNumbers_OrderId");
        
        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}