using CryptoJackpot.Audit.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CryptoJackpot.Audit.Domain.Models;

/// <summary>
/// Represents an audit log entry that captures system activities and events.
/// </summary>
public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of audit event.
    /// </summary>
    [BsonElement("eventType")]
    [BsonRepresentation(BsonType.String)]
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// Source microservice that generated this event.
    /// </summary>
    [BsonElement("source")]
    [BsonRepresentation(BsonType.String)]
    public AuditSource Source { get; set; }

    /// <summary>
    /// Outcome status of the operation.
    /// </summary>
    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public AuditStatus Status { get; set; }

    /// <summary>
    /// Correlation ID to trace related events across microservices.
    /// </summary>
    [BsonElement("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User ID associated with this event (null for system events).
    /// </summary>
    [BsonElement("userId")]
    public Guid? UserId { get; set; }

    /// <summary>
    /// Username for display purposes.
    /// </summary>
    [BsonElement("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Action performed (e.g., "CreateTransaction", "Login").
    /// </summary>
    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Description of the event.
    /// </summary>
    [BsonElement("description")]
    public string? Description { get; set; }

    /// <summary>
    /// HTTP request information.
    /// </summary>
    [BsonElement("request")]
    public AuditRequestInfo? Request { get; set; }

    /// <summary>
    /// Response information including status code and duration.
    /// </summary>
    [BsonElement("response")]
    public AuditResponseInfo? Response { get; set; }

    /// <summary>
    /// Type of resource being acted upon (e.g., "Transaction", "User").
    /// </summary>
    [BsonElement("resourceType")]
    public string? ResourceType { get; set; }

    /// <summary>
    /// ID of the resource being acted upon.
    /// </summary>
    [BsonElement("resourceId")]
    public string? ResourceId { get; set; }

    /// <summary>
    /// Previous state of the resource (for update operations).
    /// </summary>
    [BsonElement("oldValue")]
    public BsonDocument? OldValue { get; set; }

    /// <summary>
    /// New state of the resource (for create/update operations).
    /// </summary>
    [BsonElement("newValue")]
    public BsonDocument? NewValue { get; set; }

    /// <summary>
    /// Additional metadata specific to the event type.
    /// </summary>
    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace for error events.
    /// </summary>
    [BsonElement("stackTrace")]
    public string? StackTrace { get; set; }
}

