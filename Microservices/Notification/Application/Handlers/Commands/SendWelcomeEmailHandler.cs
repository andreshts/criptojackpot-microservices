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

/// <summary>
/// Handles sending welcome emails to users who registered via external providers (Google).
/// </summary>
public class SendWelcomeEmailHandler : IRequestHandler<SendWelcomeEmailCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendWelcomeEmailHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendWelcomeEmailCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.WelcomeEmail);
        if (template == null)
        {
            // Fallback: if no welcome template exists, just log and succeed
            _logger.LogWarning("Welcome email template not found, skipping email for user {UserId}", request.UserId);
            return Result.Ok(true);
        }

        var fullName = $"{request.Name} {request.LastName}";
        var dashboardUrl = $"{_config.Brevo!.BaseUrl}/dashboard";

        var body = template
            .Replace("{0}", fullName)
            .Replace("{1}", DateTime.Now.ToString("MM/dd/yyyy"))
            .Replace("{2}", dashboardUrl);

        var subject = $"Welcome to CryptoJackpot, {fullName}!";
        var success = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.WelcomeEmail,
            Success = success,
            ErrorMessage = success ? null : "Failed to send",
            SentAt = DateTime.UtcNow
        });

        if (!success)
        {
            _logger.LogError("Failed to send welcome email to {Email}", request.Email);
            return Result.Fail<bool>(new ExternalServiceError("Brevo", "Failed to send welcome email"));
        }

        _logger.LogInformation("Welcome email sent to {Email}", request.Email);
        return Result.Ok(true);
    }
}

