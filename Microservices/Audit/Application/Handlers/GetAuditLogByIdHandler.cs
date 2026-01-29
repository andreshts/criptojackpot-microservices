using AutoMapper;
using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Application.Queries;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Handlers;

/// <summary>
/// Handler for getting a specific audit log by ID.
/// </summary>
public class GetAuditLogByIdHandler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;

    public GetAuditLogByIdHandler(IAuditLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var log = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (log == null)
            {
                return Result.Fail<AuditLogDto>(new NotFoundError("Audit log not found"));
            }

            var dto = _mapper.Map<AuditLogDto>(log);
            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result.Fail<AuditLogDto>(
                new InternalServerError($"Failed to retrieve audit log: {ex.Message}"));
        }
    }
}
