using Asp.Versioning;
using CryptoJackpot.Audit.Application.Queries;
using CryptoJackpot.Domain.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Audit.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets audit logs with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a specific audit log by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLogById(string id)
    {
        var result = await _mediator.Send(new GetAuditLogByIdQuery(id));
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets audit logs by correlation ID to trace related events.
    /// </summary>
    [HttpGet("trace/{correlationId}")]
    public async Task<IActionResult> GetAuditTrace(string correlationId)
    {
        var result = await _mediator.Send(new GetAuditTraceQuery(correlationId));
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets audit log statistics/counts.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] GetAuditStatsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }
}
