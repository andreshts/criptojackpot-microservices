using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;

namespace CryptoJackpot.Domain.Core.Services;

/// <summary>
/// Service for publishing audit log events from any microservice.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Publishes an audit log event.
    /// </summary>
    Task LogAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and publishes an audit log event with the provided parameters.
    /// </summary>
    Task LogAsync(
        int eventType,
        int source,
        string action,
        int status = 1,
        Guid? userId = null,
        string? username = null,
        string? description = null,
        string? correlationId = null,
        string? endpoint = null,
        string? httpMethod = null,
        string? ipAddress = null,
        string? userAgent = null,
        int? statusCode = null,
        long? durationMs = null,
        string? resourceType = null,
        string? resourceId = null,
        object? oldValue = null,
        object? newValue = null,
        object? metadata = null,
        string? errorMessage = null,
        string? stackTrace = null,
        CancellationToken cancellationToken = default);
}
