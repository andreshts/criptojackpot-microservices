using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Api.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

/// <summary>
/// Controller for Two-Factor Authentication setup and management.
/// All endpoints require authentication.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/2fa")]
[Authorize]
public class TwoFactorController : ControllerBase
{
    private readonly IMediator _mediator;

    public TwoFactorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Initiate 2FA setup. Generates secret and QR code URI.
    /// User must confirm with /confirm endpoint before 2FA is active.
    /// </summary>
    /// <returns>Secret and QR code URI for authenticator app setup</returns>
    [HttpPost("setup")]
    public async Task<IActionResult> Setup()
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new Setup2FaCommand { UserGuid = userGuid.Value };
        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return result.ToActionResult();

        return Ok(new
        {
            success = true,
            data = result.Value
        });
    }

    /// <summary>
    /// Confirm 2FA setup by validating TOTP code from authenticator app.
    /// On success, enables 2FA and returns recovery codes (shown only once!).
    /// </summary>
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] Confirm2FaRequest request)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new Confirm2FaCommand
        {
            UserGuid = userGuid.Value,
            Code = request.Code
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return result.ToActionResult();

        return Ok(new
        {
            success = true,
            message = "2FA enabled successfully. Save your recovery codes in a safe place.",
            data = result.Value
        });
    }

    /// <summary>
    /// Disable 2FA. Requires TOTP code or recovery code for verification.
    /// </summary>
    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] Disable2FaRequest request)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new Disable2FaCommand
        {
            UserGuid = userGuid.Value,
            Code = request.Code,
            RecoveryCode = request.RecoveryCode
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return result.ToActionResult();

        return Ok(new
        {
            success = true,
            message = "2FA has been disabled."
        });
    }
}
