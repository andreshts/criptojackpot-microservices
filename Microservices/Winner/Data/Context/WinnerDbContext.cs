using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Winner.Data.Context;

public class WinnerDbContext : DbContext
{
    public WinnerDbContext(DbContextOptions<WinnerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MassTransit Outbox configuration
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
