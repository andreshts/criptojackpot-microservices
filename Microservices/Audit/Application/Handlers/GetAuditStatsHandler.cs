using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Application.Queries;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Handlers;

/// <summary>
/// Handler for getting audit log statistics.
/// </summary>
public class GetAuditStatsHandler : IRequestHandler<GetAuditStatsQuery, Result<AuditStatsDto>>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditStatsHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AuditStatsDto>> Handle(GetAuditStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var count = await _repository.CountAsync(
                request.EventType,
                request.Source,
                request.UserId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

            return Result.Ok(new AuditStatsDto { Count = count });
        }
        catch (Exception ex)
        {
            return Result.Fail<AuditStatsDto>(
                new InternalServerError($"Failed to retrieve audit stats: {ex.Message}"));
        }
    }
}
