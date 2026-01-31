using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

public class IdentityEventPublisher : IIdentityEventPublisher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<IdentityEventPublisher> _logger;

    public IdentityEventPublisher(IEventBus eventBus, ILogger<IdentityEventPublisher> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task PublishUserRegisteredAsync(User user)
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
                ConfirmationToken = user.EmailVerificationToken!
            });
            _logger.LogInformation("UserRegisteredEvent published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent for user {UserId}", user.Id);
        }
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

    public async Task PublishPasswordResetRequestedAsync(User user, string securityCode)
    {
        try
        {
            await _eventBus.Publish(new PasswordResetRequestedEvent
            {
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                SecurityCode = securityCode
            });
            _logger.LogInformation("PasswordResetRequestedEvent published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PasswordResetRequestedEvent for user {UserId}", user.Id);
        }
    }
}
