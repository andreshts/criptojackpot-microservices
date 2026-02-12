using CryptoJackpot.Identity.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Context;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<UserReferral> UserReferrals { get; set; } = null!;
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
    public DbSet<UserRecoveryCode> UserRecoveryCodes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        
        // Configure MassTransit Outbox tables with snake_case naming
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state"));
    }
}
