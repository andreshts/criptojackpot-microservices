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
/// Handler for sending lottery marketing emails to users.
/// </summary>
public class SendLotteryMarketingEmailHandler : IRequestHandler<SendLotteryMarketingEmailCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendLotteryMarketingEmailHandler> _logger;

    public SendLotteryMarketingEmailHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendLotteryMarketingEmailHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendLotteryMarketingEmailCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.LotteryMarketing);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.LotteryMarketing);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.LotteryMarketing}"));
        }

        var lotteryUrl = $"{_config.Brevo!.BaseUrl}{UrlPaths.LotteryDetails}/{request.LotteryId}";
        var fullName = $"{request.UserName} {request.UserLastName}";

        var body = template
            .Replace("{UserName}", fullName)
            .Replace("{LotteryTitle}", request.LotteryTitle)
            .Replace("{LotteryDescription}", request.LotteryDescription)
            .Replace("{TicketPrice}", request.TicketPrice.ToString("C"))
            .Replace("{StartDate}", request.StartDate.ToString("MMMM dd, yyyy"))
            .Replace("{EndDate}", request.EndDate.ToString("MMMM dd, yyyy"))
            .Replace("{MaxTickets}", request.MaxTickets.ToString("N0"))
            .Replace("{LotteryUrl}", lotteryUrl);

        var subject = $"🎰 New Lottery Alert: {request.LotteryTitle} - Don't Miss Out!";
        var success = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.LotteryMarketing,
            Success = success,
            ErrorMessage = success ? null : "Failed to send marketing email",
            SentAt = DateTime.UtcNow
        });

        if (!success)
        {
            _logger.LogWarning("Failed to send lottery marketing email to {Email} for lottery {LotteryId}", 
                request.Email, request.LotteryId);
            return Result.Fail<bool>(new InternalServerError("Failed to send marketing email"));
        }

        _logger.LogDebug("Lottery marketing email sent successfully to {Email} for lottery {LotteryId}", 
            request.Email, request.LotteryId);
        return Result.Ok(true);
    }
}
