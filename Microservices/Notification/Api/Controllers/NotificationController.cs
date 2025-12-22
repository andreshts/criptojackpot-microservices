using CryptoJackpot.Notification.Api.Extensions;
using CryptoJackpot.Notification.Application.DTOs;
using CryptoJackpot.Notification.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Notification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("email-confirmation")]
    public async Task<IActionResult> SendEmailConfirmation([FromBody] SendEmailConfirmationRequest request)
    {
        var result = await _notificationService.SendEmailConfirmationAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> SendPasswordReset([FromBody] SendPasswordResetRequest request)
    {
        var result = await _notificationService.SendPasswordResetEmailAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("referral-notification")]
    public async Task<IActionResult> SendReferralNotification([FromBody] SendReferralNotificationRequest request)
    {
        var result = await _notificationService.SendReferralNotificationAsync(request);
        return result.ToActionResult();
    }
}
