using CryptoJackpot.Audit.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Queries;

/// <summary>
/// Query to get audit logs by correlation ID for tracing.
/// </summary>
public class GetAuditTraceQuery : IRequest<Result<IEnumerable<AuditLogDto>>>
{
    public string CorrelationId { get; set; } = string.Empty;

    public GetAuditTraceQuery(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
