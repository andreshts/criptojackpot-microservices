using CryptoJackpot.Audit.Domain.Enums;
using CryptoJackpot.Audit.Domain.Models;

namespace CryptoJackpot.Audit.Domain.Interfaces;

/// <summary>
/// Repository interface for audit log operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple audit log entries in batch.
    /// </summary>
    Task CreateManyAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an audit log by its ID.
    /// </summary>
    Task<AuditLog?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by correlation ID.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by event type within a date range.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEventTypeAsync(
        AuditEventType eventType,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by source microservice.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetBySourceAsync(
        AuditSource source,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs within a date range.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific resource.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByResourceAsync(
        string resourceType,
        string resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts audit logs matching the specified criteria.
    /// </summary>
    Task<long> CountAsync(
        AuditEventType? eventType = null,
        AuditSource? source = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
