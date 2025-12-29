using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserReferralController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserReferralController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetUserReferrals([FromRoute] long userId)
    {
        var query = new GetReferralStatsQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }
}
