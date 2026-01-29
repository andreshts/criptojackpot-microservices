using AutoMapper;
using CryptoJackpot.Audit.Application.Commands;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Audit.Domain.Models;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Handlers;

/// <summary>
/// Handler for creating audit log entries.
/// </summary>
public class CreateAuditLogHandler : IRequestHandler<CreateAuditLogCommand, Result<string>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;

    public CreateAuditLogHandler(IAuditLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<string>> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = _mapper.Map<AuditLog>(request);

            await _repository.CreateAsync(auditLog, cancellationToken);

            return Result.Ok(auditLog.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(new InternalServerError($"Failed to create audit log: {ex.Message}"));
        }
    }
}
