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
    
    /// <summary>
    /// Publishes user registered event for local registration (requires email verification).
    /// </summary>
    Task PublishUserRegisteredAsync(User user, string confirmationToken);
    
    /// <summary>
    /// Publishes user registered event for external registration (Google OAuth - email pre-verified).
    /// </summary>
    Task PublishExternalUserRegisteredAsync(User user);
    
    Task PublishUserLockedOutAsync(User user, int lockoutMinutes, string? ipAddress, string? userAgent);
    Task PublishSecurityAlertAsync(User user, SecurityAlertType alertType, string description, string? ipAddress, string? userAgent);
    
    /// <summary>
    /// Publishes a security alert when User object is not available (e.g., token reuse detection).
    /// </summary>
    Task PublishSecurityAlertAsync(long userId, string email, SecurityAlertType alertType, string? ipAddress);
}
