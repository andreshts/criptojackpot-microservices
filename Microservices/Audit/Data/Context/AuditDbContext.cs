using CryptoJackpot.Audit.Data.Configuration;
using CryptoJackpot.Audit.Domain.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CryptoJackpot.Audit.Data.Context;

/// <summary>
/// MongoDB context for audit database operations.
/// </summary>
public class AuditDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public AuditDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    /// <summary>
    /// Gets the audit logs collection.
    /// </summary>
    public IMongoCollection<AuditLog> AuditLogs =>
        _database.GetCollection<AuditLog>(_settings.AuditLogsCollection);

    /// <summary>
    /// Ensures required indexes are created.
    /// </summary>
    public async Task EnsureIndexesCreatedAsync()
    {
        // First, get existing indexes to avoid conflicts
        var existingIndexes = new HashSet<string>();
        try
        {
            using var cursor = await AuditLogs.Indexes.ListAsync();
            var indexes = await cursor.ToListAsync();
            foreach (var index in indexes)
            {
                if (index.Contains("name"))
                {
                    existingIndexes.Add(index["name"].AsString);
                }
            }
        }
        catch
        {
            // Collection might not exist yet, that's fine
        }

        var indexModels = new List<CreateIndexModel<AuditLog>>();

        // Index on timestamp for time-based queries
        if (!existingIndexes.Contains("idx_timestamp") && !existingIndexes.Contains("timestamp_-1"))
        {
            indexModels.Add(new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Descending(x => x.Timestamp),
                new CreateIndexOptions { Name = "idx_timestamp" }
            ));
        }

        // Index on userId for user-specific queries
        if (!existingIndexes.Contains("idx_userId"))
        {
            indexModels.Add(new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(x => x.UserId),
                new CreateIndexOptions { Name = "idx_userId", Sparse = true }
            ));
        }

        // Index on correlationId for tracing related events
        if (!existingIndexes.Contains("idx_correlationId"))
        {
            indexModels.Add(new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(x => x.CorrelationId),
                new CreateIndexOptions { Name = "idx_correlationId", Sparse = true }
            ));
        }

        // Compound index for common query patterns
        if (!existingIndexes.Contains("idx_source_eventType_timestamp"))
        {
            indexModels.Add(new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys
                    .Ascending("source")
                    .Ascending("eventType")
                    .Descending(x => x.Timestamp),
                new CreateIndexOptions { Name = "idx_source_eventType_timestamp" }
            ));
        }

        // Index on resource for resource-specific queries
        if (!existingIndexes.Contains("idx_resource"))
        {
            indexModels.Add(new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys
                    .Ascending(x => x.ResourceType)
                    .Ascending(x => x.ResourceId),
                new CreateIndexOptions { Name = "idx_resource", Sparse = true }
            ));
        }

        // TTL index for automatic document expiration
        if (_settings.RetentionDays > 0 && !existingIndexes.Contains("idx_ttl") && !existingIndexes.Contains("timestamp_1"))
        {
            indexModels.Add(new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(x => x.Timestamp),
                new CreateIndexOptions
                {
                    Name = "idx_ttl",
                    ExpireAfter = TimeSpan.FromDays(_settings.RetentionDays)
                }
            ));
        }

        if (indexModels.Count > 0)
        {
            try
            {
                await AuditLogs.Indexes.CreateManyAsync(indexModels);
            }
            catch (MongoCommandException ex) when (ex.Message.Contains("Index already exists"))
            {
                // Index already exists with different name, that's acceptable
                // Log and continue
            }
        }
    }
}
