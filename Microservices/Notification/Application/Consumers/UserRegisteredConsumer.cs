using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IMediator mediator, ILogger<UserRegisteredConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received UserRegisteredEvent for user {UserId}", message.UserId);

        // Skip email verification for external registrations (Google OAuth)
        // or if email is already verified
        if (message.IsExternalRegistration || message.EmailVerified)
        {
            _logger.LogInformation(
                "Skipping email verification for user {UserId} - External: {IsExternal}, Verified: {Verified}",
                message.UserId, message.IsExternalRegistration, message.EmailVerified);
            
            // Optionally send a welcome email instead
            await _mediator.Send(new SendWelcomeEmailCommand
            {
                UserId = message.UserId,
                Email = message.Email,
                Name = message.Name,
                LastName = message.LastName
            });
            return;
        }

        await _mediator.Send(new SendEmailConfirmationCommand
        {
            UserId = message.UserId,
            Email = message.Email,
            Name = message.Name,
            LastName = message.LastName,
            Token = message.ConfirmationToken!
        });
    }
}
