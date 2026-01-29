using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Audit;

/// <summary>
/// Integration event for audit logging.
/// Published by all microservices to log their activities.
/// Consumed by: Audit microservice
/// </summary>
public class AuditLogEvent : Event
{
    /// <summary>
    /// Event type identifier (e.g., "UserLogin", "TransactionCreated")
    /// </summary>
    public int EventType { get; set; }

    /// <summary>
    /// Source microservice identifier (e.g., 1=Identity, 2=Wallet)
    /// </summary>
    public int Source { get; set; }

    /// <summary>
    /// Status of the operation (e.g., 1=Success, 2=Failed)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Correlation ID for tracing related events
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User ID if applicable
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Username for display
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Action performed
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// HTTP endpoint if applicable
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// HTTP method (GET, POST, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Client IP address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Client user agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// HTTP status code of response
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Duration of operation in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Type of resource affected (e.g., "Transaction", "User")
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// ID of the affected resource
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Previous value (JSON serialized) for update operations
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value (JSON serialized) for create/update operations
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Additional metadata (JSON serialized)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace for errors
    /// </summary>
    public string? StackTrace { get; set; }
}
