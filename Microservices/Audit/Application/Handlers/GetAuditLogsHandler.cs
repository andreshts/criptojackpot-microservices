using AutoMapper;
using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Application.Queries;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Audit.Application.Handlers;

/// <summary>
/// Handler for getting audit logs with filters.
/// </summary>
public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<IEnumerable<AuditLogDto>>>
{
    private readonly IAuditLogRepository _repository;
    private readonly IMapper _mapper;

    public GetAuditLogsHandler(IAuditLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var logs = await GetLogsAsync(request, cancellationToken);
            var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Result.Fail<IEnumerable<AuditLogDto>>(
                new InternalServerError($"Failed to retrieve audit logs: {ex.Message}"));
        }
    }

    private async Task<IEnumerable<Domain.Models.AuditLog>> GetLogsAsync(
        GetAuditLogsQuery request, 
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.CorrelationId))
        {
            return await _repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        }

        if (!string.IsNullOrEmpty(request.ResourceType) && !string.IsNullOrEmpty(request.ResourceId))
        {
            return await _repository.GetByResourceAsync(request.ResourceType, request.ResourceId, cancellationToken);
        }

        if (request.UserId.HasValue)
        {
            return await _repository.GetByUserIdAsync(
                request.UserId.Value,
                request.Page,
                request.PageSize,
                cancellationToken);
        }

        if (request.EventType.HasValue)
        {
            return await _repository.GetByEventTypeAsync(
                request.EventType.Value,
                request.StartDate,
                request.EndDate,
                request.Page,
                request.PageSize,
                cancellationToken);
        }

        if (request.Source.HasValue)
        {
            return await _repository.GetBySourceAsync(
                request.Source.Value,
                request.StartDate,
                request.EndDate,
                request.Page,
                request.PageSize,
                cancellationToken);
        }

        // Default: last 24 hours
        var startDate = request.StartDate ?? DateTime.UtcNow.AddHours(-24);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        return await _repository.GetByDateRangeAsync(
            startDate,
            endDate,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
