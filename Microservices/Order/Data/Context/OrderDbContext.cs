using CryptoJackpot.Order.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Order.Data.Context;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Models.Order> Orders => Set<Domain.Models.Order>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        // MassTransit Outbox configuration with snake_case naming
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state"));
    }
}
