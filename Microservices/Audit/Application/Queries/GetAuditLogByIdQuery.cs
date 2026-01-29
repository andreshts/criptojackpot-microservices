using CryptoJackpot.Audit.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Queries;

/// <summary>
/// Query to get a specific audit log by ID.
/// </summary>
public class GetAuditLogByIdQuery : IRequest<Result<AuditLogDto>>
{
    public string Id { get; set; } = string.Empty;

    public GetAuditLogByIdQuery(string id)
    {
        Id = id;
    }
}
