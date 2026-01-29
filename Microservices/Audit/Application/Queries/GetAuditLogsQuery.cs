using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Queries;

/// <summary>
/// Query to get audit logs with filters.
/// </summary>
public class GetAuditLogsQuery : IRequest<Result<IEnumerable<AuditLogDto>>>
{
    public AuditEventType? EventType { get; set; }
    public AuditSource? Source { get; set; }
    public Guid? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
