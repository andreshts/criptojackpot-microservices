using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CryptoJackpot.Order.Data.Context;

/// <summary>
/// Design-time factory for OrderDbContext.
/// Used by Entity Framework Tools for migrations.
/// </summary>
public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        
        // Use the default connection string for migrations
        // This matches the local development environment
        var connectionString = "Host=localhost;Port=5433;Database=cryptojackpot_order_db;Username=postgres;Password=postgres;";
        
        optionsBuilder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new OrderDbContext(optionsBuilder.Options);
    }
}
