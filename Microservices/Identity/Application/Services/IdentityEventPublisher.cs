using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Publishes identity-related events to the event bus (Kafka via MassTransit).
/// </summary>
public class IdentityEventPublisher : IIdentityEventPublisher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<IdentityEventPublisher> _logger;

    public IdentityEventPublisher(IEventBus eventBus, ILogger<IdentityEventPublisher> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task PublishReferralCreatedAsync(User referrer, User referred, string referralCode)
    {
        try
        {
            await _eventBus.Publish(new ReferralCreatedEvent
            {
                ReferrerEmail = referrer.Email,
                ReferrerName = referrer.Name,
                ReferrerLastName = referrer.LastName,
                ReferredName = referred.Name,
                ReferredLastName = referred.LastName,
                ReferralCode = referralCode
            });
            _logger.LogInformation("ReferralCreatedEvent published for referrer {ReferrerId}", referrer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ReferralCreatedEvent for referrer {ReferrerId}", referrer.Id);
        }
    }

    public async Task PublishUserLoggedInAsync(User user)
    {
        try
        {
            await _eventBus.Publish(new UserLoggedInEvent(user.Id, user.Email, $"{user.Name} {user.LastName}"));
            _logger.LogInformation("UserLoggedInEvent published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserLoggedInEvent for user {UserId}", user.Id);
        }
    }

    public async Task PublishUserRegisteredAsync(User user, string confirmationToken)
    {
        try
        {
            await _eventBus.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                UserGuid = user.UserGuid,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                ConfirmationToken = confirmationToken,
                IsExternalRegistration = false,
                EmailVerified = user.EmailVerified
            });
            _logger.LogInformation("UserRegisteredEvent published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent for user {UserId}", user.Id);
        }
    }

    public async Task PublishExternalUserRegisteredAsync(User user)
    {
        try
        {
            await _eventBus.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                UserGuid = user.UserGuid,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                ConfirmationToken = null, // No token needed - email verified by provider
                IsExternalRegistration = true,
                EmailVerified = user.EmailVerified
            });
            _logger.LogInformation("UserRegisteredEvent (external) published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent (external) for user {UserId}", user.Id);
        }
    }

    public async Task PublishUserLockedOutAsync(User user, int lockoutMinutes, string? ipAddress, string? userAgent)
    {
        try
        {
            await _eventBus.Publish(new UserLockedOutEvent
            {
                UserGuid = user.UserGuid,
                Email = user.Email,
                Name = user.Name,
                FailedAttempts = user.FailedLoginAttempts,
                LockoutMinutes = lockoutMinutes,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });
            _logger.LogInformation("UserLockedOutEvent published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserLockedOutEvent for user {UserId}", user.Id);
        }
    }

    public async Task PublishSecurityAlertAsync(User user, SecurityAlertType alertType, string description, string? ipAddress, string? userAgent)
    {
        try
        {
            await _eventBus.Publish(new SecurityAlertEvent
            {
                UserGuid = user.UserGuid,
                Email = user.Email,
                Name = user.Name,
                AlertType = alertType,
                Description = description,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });
            _logger.LogInformation("SecurityAlertEvent ({AlertType}) published for user {UserId}", alertType, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SecurityAlertEvent for user {UserId}", user.Id);
        }
    }

    public async Task PublishSecurityAlertAsync(long userId, string email, SecurityAlertType alertType, string? ipAddress)
    {
        try
        {
            await _eventBus.Publish(new SecurityAlertEvent
            {
                UserGuid = Guid.Empty, // Not available in this context
                Email = email,
                Name = null,
                AlertType = alertType,
                Description = $"Security alert: {alertType}",
                IpAddress = ipAddress,
                UserAgent = null
            });
            _logger.LogInformation("SecurityAlertEvent ({AlertType}) published for userId {UserId}", alertType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SecurityAlertEvent for userId {UserId}", userId);
        }
    }
}
