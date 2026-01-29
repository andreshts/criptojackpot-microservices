using CryptoJackpot.Audit.Domain.Helpers;
using CryptoJackpot.Audit.Domain.Enums;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Audit.Domain.Models;
using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Audit.Application.Consumers;

/// <summary>
/// Consumes audit log events from Kafka and persists them to MongoDB.
/// </summary>
public class AuditLogEventConsumer : IConsumer<AuditLogEvent>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogEventConsumer> _logger;

    public AuditLogEventConsumer(
        IAuditLogRepository repository,
        ILogger<AuditLogEventConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditLogEvent> context)
    {
        var message = context.Message;
        
        _logger.LogDebug(
            "Received AuditLogEvent: {EventType} from {Source} for action {Action}",
            message.EventType,
            message.Source,
            message.Action);

        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = message.Timestamp,
                EventType = (AuditEventType)message.EventType,
                Source = (AuditSource)message.Source,
                Status = (AuditStatus)message.Status,
                CorrelationId = message.CorrelationId,
                UserId = message.UserId,
                Username = message.Username,
                Action = message.Action,
                Description = message.Description,
                ResourceType = message.ResourceType,
                ResourceId = message.ResourceId,
                ErrorMessage = message.ErrorMessage,
                StackTrace = message.StackTrace,
                Request = new AuditRequestInfo
                {
                    Endpoint = message.Endpoint,
                    Method = message.HttpMethod,
                    IpAddress = message.IpAddress,
                    UserAgent = message.UserAgent
                },
                Response = new AuditResponseInfo
                {
                    StatusCode = message.StatusCode,
                    DurationMs = message.DurationMs
                },
                OldValue = BsonHelper.ParseFromJson(message.OldValue),
                NewValue = BsonHelper.ParseFromJson(message.NewValue),
                Metadata = BsonHelper.ParseFromJson(message.Metadata)
            };

            await _repository.CreateAsync(auditLog, context.CancellationToken);

            _logger.LogInformation(
                "Audit log created: {Id} for action {Action}",
                auditLog.Id,
                auditLog.Action);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process AuditLogEvent for action {Action}",
                message.Action);
            throw;
        }
    }
}
