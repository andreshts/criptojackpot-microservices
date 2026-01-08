using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

public class SendReferralNotificationCommand : IRequest<Result<bool>>
{
    public string ReferrerEmail { get; set; } = null!;
    public string ReferrerName { get; set; } = null!;
    public string ReferrerLastName { get; set; } = null!;
    public string ReferredName { get; set; } = null!;
    public string ReferredLastName { get; set; } = null!;
    public string ReferralCode { get; set; } = null!;
}
