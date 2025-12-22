using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.DTOs;
using CryptoJackpot.Notification.Application.Interfaces;
using MediatR;

namespace CryptoJackpot.Notification.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IMediator _mediator;

    public NotificationService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ResultResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request)
    {
        return await _mediator.Send(new SendEmailConfirmationCommand
        {
            UserId = request.UserId,
            Email = request.Email,
            Name = request.Name,
            LastName = request.LastName,
            Token = request.Token
        });
    }

    public async Task<ResultResponse<bool>> SendPasswordResetEmailAsync(SendPasswordResetRequest request)
    {
        return await _mediator.Send(new SendPasswordResetCommand
        {
            Email = request.Email,
            Name = request.Name,
            LastName = request.LastName,
            SecurityCode = request.SecurityCode
        });
    }

    public async Task<ResultResponse<bool>> SendReferralNotificationAsync(SendReferralNotificationRequest request)
    {
        return await _mediator.Send(new SendReferralNotificationCommand
        {
            ReferrerEmail = request.ReferrerEmail,
            ReferrerName = request.ReferrerName,
            ReferrerLastName = request.ReferrerLastName,
            ReferredName = request.ReferredName,
            ReferredLastName = request.ReferredLastName,
            ReferralCode = request.ReferralCode
        });
    }
}
