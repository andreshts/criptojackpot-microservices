using CryptoJackpot.Identity.Application.Events;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Events;

/// <summary>
/// Handles referral creation when a user is created with a referral code.
/// Decoupled from CreateUserCommandHandler via domain events.
/// Publishes ReferralCreatedEvent to Kafka for Notification and Wallet services.
/// </summary>
public class ProcessReferralHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly IUserReferralRepository _userReferralRepository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly ILogger<ProcessReferralHandler> _logger;

    public ProcessReferralHandler(
        IUserReferralRepository userReferralRepository,
        IIdentityEventPublisher eventPublisher,
        ILogger<ProcessReferralHandler> logger)
    {
        _userReferralRepository = userReferralRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Referrer is null || string.IsNullOrEmpty(notification.ReferralCode))
        {
            _logger.LogDebug("No referral to process for user {UserId}", notification.User.Id);
            return;
        }

        try
        {
            var userReferral = new UserReferral
            {
                ReferrerId = notification.Referrer.Id,
                ReferredId = notification.User.Id,
                UsedSecurityCode = notification.ReferralCode
            };

            await _userReferralRepository.CreateUserReferralAsync(userReferral);

            // Publish event to Kafka for Notification and Wallet services
            await _eventPublisher.PublishReferralCreatedAsync(
                notification.Referrer, 
                notification.User, 
                notification.ReferralCode);

            _logger.LogInformation(
                "Referral created: User {ReferredId} referred by {ReferrerId}", 
                notification.User.Id, 
                notification.Referrer.Id);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - this is fire-and-forget
            // The user creation should not fail due to referral processing errors
            _logger.LogError(ex, 
                "Failed to process referral for user {UserId}. Referrer: {ReferrerId}", 
                notification.User.Id,
                notification.Referrer.Id);
        }
    }
}

