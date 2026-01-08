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

public class SendReferralNotificationHandler : IRequestHandler<SendReferralNotificationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendReferralNotificationHandler> _logger;

    public SendReferralNotificationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendReferralNotificationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendReferralNotificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.ReferralNotification);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.ReferralNotification);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.ReferralNotification}"));
        }

        var referrerFullName = $"{request.ReferrerName} {request.ReferrerLastName}";
        var referredFullName = $"{request.ReferredName} {request.ReferredLastName}";
        var referralsUrl = $"{_config.Brevo!.BaseUrl}{UrlPaths.ReferralProgram}";

        var body = template
            .Replace("{0}", referrerFullName)
            .Replace("{1}", referredFullName)
            .Replace("{2}", request.ReferralCode)
            .Replace("{3}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"))
            .Replace("{4}", referralsUrl);

        var subject = "New Referral - CryptoJackpot";
        var success = await _emailProvider.SendEmailAsync(request.ReferrerEmail, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.ReferrerEmail,
            Subject = subject,
            TemplateName = TemplateNames.ReferralNotification,
            Success = success,
            ErrorMessage = success ? null : "Failed to send email",
            SentAt = DateTime.UtcNow
        });

        if (!success)
        {
            _logger.LogWarning("Failed to send referral notification to {Email}", request.ReferrerEmail);
            return Result.Fail<bool>(new InternalServerError("Failed to send referral notification"));
        }

        _logger.LogInformation("Referral notification sent successfully to {Email}", request.ReferrerEmail);
        return Result.Ok(true);
    }
}
