using AutoMapper;
using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Application.Queries;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Handlers;

/// <summary>
/// Handler for getting audit logs by correlation ID.
/// </summary>
public class GetAuditTraceHandler : IRequestHandler<GetAuditTraceQuery, Result<IEnumerable<AuditLogDto>>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;

    public GetAuditTraceHandler(IAuditLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AuditLogDto>>> Handle(GetAuditTraceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var logs = await _repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
            var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Result.Fail<IEnumerable<AuditLogDto>>(
                new InternalServerError($"Failed to retrieve audit trace: {ex.Message}"));
        }
    }
}
