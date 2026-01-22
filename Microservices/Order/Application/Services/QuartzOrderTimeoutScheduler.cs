using System.Text.Json;
using CryptoJackpot.Order.Application.Interfaces;
using CryptoJackpot.Order.Application.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CryptoJackpot.Order.Application.Services;

/// <summary>
/// Implementation of order timeout scheduling using Quartz.NET with database persistence
/// </summary>
public class QuartzOrderTimeoutScheduler : IOrderTimeoutScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzOrderTimeoutScheduler> _logger;

    public QuartzOrderTimeoutScheduler(
        ISchedulerFactory schedulerFactory,
        ILogger<QuartzOrderTimeoutScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task ScheduleOrderTimeoutAsync(
        Guid orderId,
        Guid lotteryId,
        List<Guid> lotteryNumberIds,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        
        var jobKey = OrderTimeoutJob.CreateJobKey(orderId);
        var triggerKey = OrderTimeoutJob.CreateTriggerKey(orderId);

        // Check if job already exists
        if (await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning(
                "Order timeout job already exists for Order {OrderId}. Rescheduling...",
                orderId);
            await RescheduleOrderTimeoutAsync(orderId, expiresAt, cancellationToken);
            return;
        }

        // Serialize lottery number IDs to JSON for storage
        var lotteryNumberIdsJson = JsonSerializer.Serialize(lotteryNumberIds);

        // Create job with data
        var job = JobBuilder.Create<OrderTimeoutJob>()
            .WithIdentity(jobKey)
            .UsingJobData(OrderTimeoutJob.OrderIdKey, orderId.ToString())
            .UsingJobData(OrderTimeoutJob.LotteryIdKey, lotteryId.ToString())
            .UsingJobData(OrderTimeoutJob.LotteryNumberIdsKey, lotteryNumberIdsJson)
            .StoreDurably(false)
            .Build();

        // Create trigger to fire at expiration time
        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .StartAt(new DateTimeOffset(expiresAt, TimeSpan.Zero))
            .WithSimpleSchedule(x => x.WithMisfireHandlingInstructionFireNow())
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);

        _logger.LogInformation(
            "Scheduled order timeout for Order {OrderId} at {ExpiresAt:O}. LotteryNumberIds: {Count}",
            orderId, expiresAt, lotteryNumberIds.Count);
    }

    public async Task<bool> CancelOrderTimeoutAsync(
        Guid orderId, 
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = OrderTimeoutJob.CreateJobKey(orderId);

        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogDebug("Order timeout job not found for Order {OrderId}", orderId);
            return false;
        }

        var deleted = await scheduler.DeleteJob(jobKey, cancellationToken);
        
        if (deleted)
        {
            _logger.LogInformation("Cancelled order timeout job for Order {OrderId}", orderId);
        }

        return deleted;
    }

    public async Task<bool> RescheduleOrderTimeoutAsync(
        Guid orderId, 
        DateTime newExpiresAt,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var triggerKey = OrderTimeoutJob.CreateTriggerKey(orderId);

        var existingTrigger = await scheduler.GetTrigger(triggerKey, cancellationToken);
        
        if (existingTrigger == null)
        {
            _logger.LogWarning(
                "Order timeout trigger not found for Order {OrderId}. Cannot reschedule.",
                orderId);
            return false;
        }

        var newTrigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .StartAt(new DateTimeOffset(newExpiresAt, TimeSpan.Zero))
            .WithSimpleSchedule(x => x.WithMisfireHandlingInstructionFireNow())
            .ForJob(existingTrigger.JobKey)
            .Build();

        await scheduler.RescheduleJob(triggerKey, newTrigger, cancellationToken);

        _logger.LogInformation(
            "Rescheduled order timeout for Order {OrderId} to {NewExpiresAt:O}",
            orderId, newExpiresAt);

        return true;
    }
}
