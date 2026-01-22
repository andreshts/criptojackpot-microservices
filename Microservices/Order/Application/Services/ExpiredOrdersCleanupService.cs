using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Services;

/// <summary>
/// Background service that periodically checks for expired orders that weren't processed
/// by Quartz (e.g., due to service restart, missed triggers, etc.).
/// Acts as a safety net to ensure all expired orders are eventually processed.
/// </summary>
public class ExpiredOrdersCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredOrdersCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _gracePeriod = TimeSpan.FromSeconds(30); // Extra time after expiration

    public ExpiredOrdersCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredOrdersCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredOrdersCleanupService started. Check interval: {Interval}", _checkInterval);

        // Initial delay to let the application start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired orders in cleanup service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Get orders that are pending but past their expiration time
        var cutoffTime = DateTime.UtcNow.Subtract(_gracePeriod);
        var expiredOrders = await orderRepository.GetExpiredPendingOrdersAsync(cutoffTime, cancellationToken);

        if (expiredOrders.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "ExpiredOrdersCleanupService: Found {Count} expired pending orders to process",
            expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            try
            {
                // Double-check status in case it changed
                if (order.Status != OrderStatus.Pending)
                {
                    continue;
                }

                // Mark as expired
                order.Status = OrderStatus.Expired;
                await orderRepository.UpdateAsync(order);

                // Get lottery number IDs
                var lotteryNumberIds = order.OrderDetails
                    .Where(od => od.LotteryNumberId.HasValue)
                    .Select(od => od.LotteryNumberId!.Value)
                    .ToList();

                // Publish event to release numbers
                await eventBus.Publish(new OrderExpiredEvent
                {
                    OrderId = order.OrderGuid,
                    LotteryId = order.LotteryId,
                    LotteryNumberIds = lotteryNumberIds
                });

                _logger.LogInformation(
                    "ExpiredOrdersCleanupService: Expired order {OrderId}. Released {Count} numbers.",
                    order.OrderGuid, lotteryNumberIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ExpiredOrdersCleanupService: Error processing expired order {OrderId}",
                    order.OrderGuid);
            }
        }
    }
}
