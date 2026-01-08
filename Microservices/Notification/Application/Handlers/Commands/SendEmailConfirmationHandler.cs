using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Constants;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Domain.Interfaces;
using CryptoJackpot.Notification.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Notification.Application.Handlers.Commands;

public class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendEmailConfirmationHandler> _logger;

    public SendEmailConfirmationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendEmailConfirmationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.ConfirmEmail);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.ConfirmEmail);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.ConfirmEmail}"));
        }

        var url = $"{_config.Brevo!.BaseUrl}{UrlPaths.ConfirmEmail}/{request.Token}";
        var fullName = $"{request.Name} {request.LastName}";

        var body = template
            .Replace("{0}", fullName)
            .Replace("{1}", DateTime.Now.ToString("MM/dd/yyyy"))
            .Replace("{2}", url);

        var subject = $"Welcome to CryptoJackpot, {fullName}!";
        var success = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.ConfirmEmail,
            Success = success,
            ErrorMessage = success ? null : "Failed to send email",
            SentAt = DateTime.UtcNow
        });

        if (!success)
        {
            _logger.LogWarning("Failed to send confirmation email for user {UserId}", request.UserId);
            return Result.Fail<bool>(new InternalServerError("Failed to send email"));
        }

        _logger.LogInformation("Email confirmation sent successfully for user {UserId}", request.UserId);
        return Result.Ok(true);
    }
}
