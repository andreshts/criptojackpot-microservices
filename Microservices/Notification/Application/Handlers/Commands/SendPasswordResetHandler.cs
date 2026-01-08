using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.Constants;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Domain.Interfaces;
using CryptoJackpot.Notification.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Handlers.Commands;

public class SendPasswordResetHandler : IRequestHandler<SendPasswordResetCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly ILogger<SendPasswordResetHandler> _logger;

    public SendPasswordResetHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        ILogger<SendPasswordResetHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.PasswordReset);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.PasswordReset);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.PasswordReset}"));
        }

        var fullName = $"{request.Name} {request.LastName}";
        var body = template
            .Replace("{0}", fullName)
            .Replace("{1}", request.SecurityCode)
            .Replace("{2}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

        var subject = "Password Reset - CryptoJackpot";
        var success = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.PasswordReset,
            Success = success,
            ErrorMessage = success ? null : "Failed to send email",
            SentAt = DateTime.UtcNow
        });

        if (!success)
        {
            _logger.LogWarning("Failed to send password reset email to {Email}", request.Email);
            return Result.Fail<bool>(new InternalServerError("Failed to send email"));
        }

        _logger.LogInformation("Password reset email sent successfully to {Email}", request.Email);
        return Result.Ok(true);
    }
}
