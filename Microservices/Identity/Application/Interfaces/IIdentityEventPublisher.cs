using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IIdentityEventPublisher
{
    Task PublishUserRegisteredAsync(User user);
    Task PublishReferralCreatedAsync(User referrer, User referred, string referralCode);
    Task PublishUserLoggedInAsync(User user);
    Task PublishPasswordResetRequestedAsync(User user, string securityCode);
}
