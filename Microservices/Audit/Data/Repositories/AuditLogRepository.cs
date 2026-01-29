using CryptoJackpot.Audit.Data.Context;
using CryptoJackpot.Audit.Domain.Enums;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Audit.Domain.Models;
using MongoDB.Driver;

namespace CryptoJackpot.Audit.Data.Repositories;

/// <summary>
/// MongoDB implementation of the audit log repository.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _context;

    public AuditLogRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.InsertOneAsync(auditLog, cancellationToken: cancellationToken);
    }

    public async Task CreateManyAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
    {
        var logs = auditLogs.ToList();
        if (logs.Count > 0)
        {
            await _context.AuditLogs.InsertManyAsync(logs, cancellationToken: cancellationToken);
        }
    }

    public async Task<AuditLog?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.Id, id);
        return await _context.AuditLogs.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.CorrelationId, correlationId);
        var sort = Builders<AuditLog>.Sort.Ascending(x => x.Timestamp);
        
        return await _context.AuditLogs
            .Find(filter)
            .Sort(sort)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.UserId, userId);
        var sort = Builders<AuditLog>.Sort.Descending(x => x.Timestamp);

        return await _context.AuditLogs
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByEventTypeAsync(
        AuditEventType eventType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(x => x.EventType, eventType);

        if (startDate.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, startDate.Value);

        if (endDate.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, endDate.Value);

        var sort = Builders<AuditLog>.Sort.Descending(x => x.Timestamp);

        return await _context.AuditLogs
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetBySourceAsync(
        AuditSource source,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(x => x.Source, source);

        if (startDate.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, startDate.Value);

        if (endDate.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, endDate.Value);

        var sort = Builders<AuditLog>.Sort.Descending(x => x.Timestamp);

        return await _context.AuditLogs
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Gte(x => x.Timestamp, startDate) &
                     filterBuilder.Lte(x => x.Timestamp, endDate);

        var sort = Builders<AuditLog>.Sort.Descending(x => x.Timestamp);

        return await _context.AuditLogs
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByResourceAsync(
        string resourceType,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(x => x.ResourceType, resourceType) &
                     filterBuilder.Eq(x => x.ResourceId, resourceId);

        var sort = Builders<AuditLog>.Sort.Descending(x => x.Timestamp);

        return await _context.AuditLogs
            .Find(filter)
            .Sort(sort)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(
        AuditEventType? eventType = null,
        AuditSource? source = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Empty;

        if (eventType.HasValue)
            filter &= filterBuilder.Eq(x => x.EventType, eventType.Value);

        if (source.HasValue)
            filter &= filterBuilder.Eq(x => x.Source, source.Value);

        if (userId.HasValue)
            filter &= filterBuilder.Eq(x => x.UserId, userId.Value);

        if (startDate.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, startDate.Value);

        if (endDate.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, endDate.Value);

        return await _context.AuditLogs.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
