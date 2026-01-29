using CryptoJackpot.Audit.Domain.Enums;

namespace CryptoJackpot.Audit.Application.DTOs;

/// <summary>
/// DTO for audit log responses.
/// </summary>
public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AuditRequestDto? Request { get; set; }
    public AuditResponseDto? Response { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public object? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AuditRequestDto
{
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class AuditResponseDto
{
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
}
