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
/// </summary>
public class ProcessReferralHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly ILogger<ProcessReferralHandler> _logger;

    public ProcessReferralHandler(
        IUserRepository userRepository,
        IIdentityEventPublisher eventPublisher,
        ILogger<ProcessReferralHandler> logger)
    {
        _userRepository = userRepository;
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

            notification.User.ReferredBy = userReferral;
            await _userRepository.UpdateAsync(notification.User);

            // Publish event to Kafka for Notification service
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
            _logger.LogError(ex, 
                "Failed to process referral for user {UserId}", 
                notification.User.Id);
            throw;
        }
    }
}

