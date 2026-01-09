using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CryptoJackpot.Lottery.Data.Context.Configurations;

public class PrizeImageConfiguration : IEntityTypeConfiguration<PrizeImage>
{
    public void Configure(EntityTypeBuilder<PrizeImage> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PrizeId).IsRequired();
        builder.Property(e => e.ImageUrl).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(500);
        builder.Property(e => e.Caption).IsRequired().HasColumnType(ColumnTypes.Text).HasMaxLength(200);
        builder.Property(e => e.DisplayOrder).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();


        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}