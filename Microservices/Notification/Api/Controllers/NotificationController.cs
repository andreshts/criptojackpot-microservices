using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Notification.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("email-confirmation")]
    public async Task<IActionResult> SendEmailConfirmation([FromBody] SendEmailConfirmationRequest request)
    {
        var command = new SendEmailConfirmationCommand
        {
            UserId = request.UserId,
            Email = request.Email,
            Name = request.Name,
            LastName = request.LastName,
            Token = request.Token
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> SendPasswordReset([FromBody] SendPasswordResetRequest request)
    {
        var command = new SendPasswordResetCommand
        {
            Email = request.Email,
            Name = request.Name,
            LastName = request.LastName,
            SecurityCode = request.SecurityCode
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("referral-notification")]
    public async Task<IActionResult> SendReferralNotification([FromBody] SendReferralNotificationRequest request)
    {
        var command = new SendReferralNotificationCommand
        {
            ReferrerEmail = request.ReferrerEmail,
            ReferrerName = request.ReferrerName,
            ReferrerLastName = request.ReferrerLastName,
            ReferredName = request.ReferredName,
            ReferredLastName = request.ReferredLastName,
            ReferralCode = request.ReferralCode
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
