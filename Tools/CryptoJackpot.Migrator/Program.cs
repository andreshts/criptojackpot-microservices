using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Lottery.Data.Context;
using CryptoJackpot.Notification.Data.Context;
using CryptoJackpot.Order.Data.Context;
using CryptoJackpot.Order.Data.Extensions;
using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Winner.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

/// <summary>
/// Centralized database migration tool for all microservices.
/// Runs as a Kubernetes Job before application deployments to ensure
/// all database schemas are up-to-date without race conditions.
/// </summary>
var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register all DbContexts with their connection strings from environment/secrets
RegisterDbContext<IdentityDbContext>(builder, "IDENTITY_DB_CONNECTION");
RegisterDbContext<LotteryDbContext>(builder, "LOTTERY_DB_CONNECTION");
RegisterDbContext<OrderDbContext>(builder, "ORDER_DB_CONNECTION");
RegisterDbContext<WalletDbContext>(builder, "WALLET_DB_CONNECTION");
RegisterDbContext<WinnerDbContext>(builder, "WINNER_DB_CONNECTION");
RegisterDbContext<NotificationDbContext>(builder, "NOTIFICATION_DB_CONNECTION");

var host = builder.Build();

using var scope = host.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("=== CryptoJackpot Database Migrator ===");
logger.LogInformation("Starting database migrations...");

var exitCode = 0;

try
{
    // Migrate sequentially to avoid race conditions
    // Order matters: Identity first (may have foreign key references from other services)
    var contexts = new (string Name, Func<DbContext> ContextFactory)[]
    {
        ("Identity", () => scope.ServiceProvider.GetRequiredService<IdentityDbContext>()),
        ("Lottery", () => scope.ServiceProvider.GetRequiredService<LotteryDbContext>()),
        ("Order", () => scope.ServiceProvider.GetRequiredService<OrderDbContext>()),
        ("Wallet", () => scope.ServiceProvider.GetRequiredService<WalletDbContext>()),
        ("Winner", () => scope.ServiceProvider.GetRequiredService<WinnerDbContext>()),
        ("Notification", () => scope.ServiceProvider.GetRequiredService<NotificationDbContext>()),
    };

    foreach (var (name, contextFactory) in contexts)
    {
        await MigrateContextAsync(name, contextFactory, logger);
    }

    // Provision Quartz.NET tables in Order database (required for order timeout scheduling)
    await ProvisionQuartzTablesAsync(
        () => scope.ServiceProvider.GetRequiredService<OrderDbContext>(),
        logger);

    logger.LogInformation("=== All migrations completed successfully! ===");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Migration failed with unrecoverable error.");
    exitCode = 1;
}

return exitCode;

// ============================================================================
// Helper methods
// ============================================================================

static void RegisterDbContext<TContext>(HostApplicationBuilder builder, string connectionStringKey)
    where TContext : DbContext
{
    var connectionString = builder.Configuration[connectionStringKey] ?? builder.Configuration.GetConnectionString(connectionStringKey);

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            $"Connection string '{connectionStringKey}' is not configured. " +
            $"Ensure the environment variable or configuration key is set.");
    }

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<TContext>(options =>
        options.UseNpgsql(dataSource)
            .UseSnakeCaseNamingConvention());
}

static async Task MigrateContextAsync(
    string name,
    Func<DbContext> contextFactory,
    ILogger logger,
    int maxRetries = 5)
{
    logger.LogInformation("[{Context}] Starting migration...", name);

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await using var context = contextFactory();

            // Wait for database to be ready
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("[{Context}] Database not ready (attempt {Attempt}/{Max}). Waiting...",
                    name, attempt, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                continue;
            }

            // Check pending migrations
            var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count == 0)
            {
                logger.LogInformation("[{Context}] No pending migrations.", name);
                return;
            }

            logger.LogInformation("[{Context}] Applying {Count} migrations: {Migrations}",
                name, pending.Count, string.Join(", ", pending));

            await context.Database.MigrateAsync();

            logger.LogInformation("[{Context}] Migration completed successfully.", name);
            return;
        }
        catch (PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
        {
            logger.LogWarning("[{Context}] Tables already exist (SqlState: 42P07). Skipping.", name);
            return;
        }
        catch (PostgresException ex) when (ex.SqlState == "55P03") // lock_not_available
        {
            logger.LogWarning("[{Context}] Migration lock held (SqlState: 55P03). Retry {Attempt}/{Max}...",
                name, attempt, maxRetries);
            if (attempt < maxRetries)
                await Task.Delay(TimeSpan.FromSeconds(attempt * 3));
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // unique_violation
        {
            logger.LogWarning("[{Context}] Migration already recorded (SqlState: 23505). Skipping.", name);
            return;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "[{Context}] Migration failed (attempt {Attempt}/{Max}). Retrying...",
                name, attempt, maxRetries);
            await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
        }
    }

    throw new InvalidOperationException($"Failed to migrate {name} after {maxRetries} attempts.");
}

/// <summary>
/// Provisions Quartz.NET tables in the Order database.
/// Uses CREATE TABLE IF NOT EXISTS for idempotency - safe to run multiple times.
/// These tables are required by Quartz.NET AdoJobStore for persistent job scheduling.
/// </summary>
static async Task ProvisionQuartzTablesAsync(
    Func<DbContext> orderContextFactory,
    ILogger logger)
{
    logger.LogInformation("[Quartz] Provisioning Quartz.NET tables in Order database...");

    try
    {
        await using var context = orderContextFactory();

        // All statements use IF NOT EXISTS / ON CONFLICT DO NOTHING
        // so this is fully idempotent and safe to run on every deployment
        await context.Database.ExecuteSqlRawAsync(QuartzSchemaExtensions.QuartzPostgresTables);

        logger.LogInformation("[Quartz] Quartz.NET tables provisioned successfully.");
    }
    catch (PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
    {
        logger.LogInformation("[Quartz] Tables already exist (concurrent creation). Skipping.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[Quartz] Failed to provision Quartz.NET tables. Order timeout scheduling will not work.");
        throw;
    }
}