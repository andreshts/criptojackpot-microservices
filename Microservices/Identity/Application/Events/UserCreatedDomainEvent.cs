using CryptoJackpot.Identity.Domain.Models;
using MediatR;

namespace CryptoJackpot.Identity.Application.Events;

/// <summary>
/// Internal domain event published after a user is created.
/// Handled by ProcessReferralHandler to create referral relationship.
/// </summary>
public class UserCreatedDomainEvent : INotification
{
    public User User { get; }
    public User? Referrer { get; }
    public string? ReferralCode { get; }

    public UserCreatedDomainEvent(User user, User? referrer = null, string? referralCode = null)
    {
        User = user;
        Referrer = referrer;
        ReferralCode = referralCode;
    }
}

