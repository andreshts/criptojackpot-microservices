using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Queries;

/// <summary>
/// Query to get audit log statistics.
/// </summary>
public class GetAuditStatsQuery : IRequest<Result<AuditStatsDto>>
{
    public AuditEventType? EventType { get; set; }
    public AuditSource? Source { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
