using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Notification.Application.DTOs;

namespace CryptoJackpot.Notification.Application.Interfaces;

public interface INotificationService
{
    Task<ResultResponse<bool>> SendEmailConfirmationAsync(SendEmailConfirmationRequest request);
    Task<ResultResponse<bool>> SendPasswordResetEmailAsync(SendPasswordResetRequest request);
    Task<ResultResponse<bool>> SendReferralNotificationAsync(SendReferralNotificationRequest request);
}
