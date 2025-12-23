using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        var command = new AuthenticateCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
