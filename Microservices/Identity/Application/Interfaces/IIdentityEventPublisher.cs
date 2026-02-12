using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Publishes identity-related events to the event bus (Kafka via MassTransit).
/// </summary>
public interface IIdentityEventPublisher
{
    Task PublishReferralCreatedAsync(User referrer, User referred, string referralCode);
    Task PublishUserLoggedInAsync(User user);
    Task PublishUserRegisteredAsync(User user, string confirmationToken);
    Task PublishUserLockedOutAsync(User user, int lockoutMinutes, string? ipAddress, string? userAgent);
    Task PublishSecurityAlertAsync(User user, SecurityAlertType alertType, string description, string? ipAddress, string? userAgent);
}
