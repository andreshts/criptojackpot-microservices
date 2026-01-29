using CryptoJackpot.Audit.Domain.Enums;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Audit.Domain.Models;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Audit.Application.Consumers;

/// <summary>
/// Consumes UserLoggedInEvent from Identity microservice and creates audit logs.
/// </summary>
public class UserLoggedInEventConsumer : IConsumer<UserLoggedInEvent>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<UserLoggedInEventConsumer> _logger;

    public UserLoggedInEventConsumer(
        IAuditLogRepository repository,
        ILogger<UserLoggedInEventConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var message = context.Message;

        _logger.LogDebug(
            "Received UserLoggedInEvent for user {UserId} ({Email})",
            message.UserId,
            message.Email);

        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = message.LoginTime,
                EventType = AuditEventType.UserLogin,
                Source = AuditSource.Identity,
                Status = AuditStatus.Success,
                CorrelationId = message.CorrelationId,
                Username = message.UserName,
                Action = "UserLogin",
                Description = $"User '{message.UserName}' ({message.Email}) logged in successfully",
                ResourceType = "User",
                ResourceId = message.UserId.ToString(),
                Metadata = new MongoDB.Bson.BsonDocument
                {
                    { "email", message.Email },
                    { "userName", message.UserName },
                    { "loginTime", message.LoginTime.ToString("O") }
                }
            };

            await _repository.CreateAsync(auditLog, context.CancellationToken);

            _logger.LogInformation(
                "Audit log created for UserLogin: User {UserId} ({UserName})",
                message.UserId,
                message.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create audit log for UserLogin: User {UserId}",
                message.UserId);
            throw;
        }
    }
}
