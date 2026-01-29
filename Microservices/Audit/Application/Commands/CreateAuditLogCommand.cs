using CryptoJackpot.Audit.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Commands;

/// <summary>
/// Command to create a new audit log entry.
/// </summary>
public class CreateAuditLogCommand : IRequest<Result<string>>
{
    public AuditEventType EventType { get; set; }
    public AuditSource Source { get; set; }
    public AuditStatus Status { get; set; } = AuditStatus.Success;
    public string? CorrelationId { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public object? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}
