using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CryptoJackpot.Lottery.Data.Context.Configurations;

public class LotteryConfiguration : IEntityTypeConfiguration<LotteryDraw>
{
    public void Configure(EntityTypeBuilder<LotteryDraw> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.LotteryGuid).IsRequired();
        builder.HasIndex(e => e.LotteryGuid).IsUnique();
        
        builder.Property(e => e.LotteryNo).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(50);
        builder.Property(e => e.Title).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(500);
        builder.Property(e => e.MinNumber).IsRequired();
        builder.Property(e => e.MaxNumber).IsRequired();
        builder.Property(e => e.TotalSeries).IsRequired();
        builder.Property(e => e.TicketPrice).IsRequired().HasColumnType(ColumnTypes.Decimal);
        builder.Property(e => e.MaxTickets).IsRequired();
        builder.Property(e => e.SoldTickets).IsRequired();
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EndDate).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Terms).IsRequired().HasColumnType(ColumnTypes.Text);
        builder.Property(e => e.HasAgeRestriction).IsRequired();
        builder.Property(e => e.MinimumAge);
        
        builder.Property(e => e.CryptoCurrencyId).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(20);
        builder.Property(e => e.CryptoCurrencySymbol).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(20);
        
        builder.Property(e => e.RestrictedCountries).HasColumnType(ColumnTypes.Jsonb);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.LotteryNo).IsUnique();

        builder.HasMany(e => e.Prizes)
            .WithOne(e => e.Lottery)
            .HasForeignKey(e => e.LotteryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.LotteryNumbers)
            .WithOne(e => e.Lottery)
            .HasForeignKey(e => e.LotteryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}