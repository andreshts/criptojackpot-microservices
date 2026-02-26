using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Order.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Order.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/tickets")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var query = new GetTicketsByUserQuery { UserId = userId.Value };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }
}
