using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Helpers;
using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;
using CryptoJackpot.Domain.Core.Services;

namespace CryptoJackpot.Infra.Bus;

/// <summary>
/// Implementation of the audit service that publishes events via the event bus.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IEventBus _eventBus;

    public AuditService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task LogAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default)
    {
        await _eventBus.Publish(auditEvent);
    }

    public async Task LogAsync(
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
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditLogEvent
        {
            EventType = eventType,
            Source = source,
            Status = status,
            Action = action,
            UserId = userId,
            Username = username,
            Description = description,
            CorrelationId = correlationId,
            Endpoint = endpoint,
            HttpMethod = httpMethod,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            StatusCode = statusCode,
            DurationMs = durationMs,
            ResourceType = resourceType,
            ResourceId = resourceId,
            OldValue = JsonHelper.SerializeToJson(oldValue),
            NewValue = JsonHelper.SerializeToJson(newValue),
            Metadata = JsonHelper.SerializeToJson(metadata),
            ErrorMessage = errorMessage,
            StackTrace = stackTrace
        };

        await _eventBus.Publish(auditEvent);
    }
}
