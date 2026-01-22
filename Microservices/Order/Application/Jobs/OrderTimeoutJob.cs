using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CryptoJackpot.Order.Application.Jobs;

/// <summary>
/// Quartz job that processes a single order timeout.
/// When triggered, it checks if the order is still pending and expires it if so.
/// </summary>
[DisallowConcurrentExecution]
public class OrderTimeoutJob : IJob
{
    public static readonly JobKey JobKeyPrefix = new("order-timeout", "order-expiration");
    
    // Job data keys
    public const string OrderIdKey = "OrderId";
    public const string LotteryIdKey = "LotteryId";
    public const string LotteryNumberIdsKey = "LotteryNumberIds";

    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderTimeoutJob> _logger;

    public OrderTimeoutJob(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        ILogger<OrderTimeoutJob> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;
        
        var orderIdString = dataMap.GetString(OrderIdKey);
        var lotteryIdString = dataMap.GetString(LotteryIdKey);
        dataMap.GetString(LotteryNumberIdsKey);

        if (string.IsNullOrEmpty(orderIdString) || !Guid.TryParse((string?)orderIdString, out var orderId))
        {
            _logger.LogError("OrderTimeoutJob: Invalid or missing OrderId in job data");
            return;
        }

        if (string.IsNullOrEmpty(lotteryIdString) || !Guid.TryParse((string?)lotteryIdString, out _))
        {
            _logger.LogError("OrderTimeoutJob: Invalid or missing LotteryId for Order {OrderId}", orderId);
            return;
        }

        _logger.LogInformation(
            "OrderTimeoutJob: Processing timeout for Order {OrderId}",
            orderId);

        try
        {
            var order = await _orderRepository.GetByGuidWithTrackingAsync(orderId);

            if (order is null)
            {
                _logger.LogWarning("OrderTimeoutJob: Order {OrderId} not found", orderId);
                return;
            }

            // Only process if order is still pending
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogInformation(
                    "OrderTimeoutJob: Order {OrderId} is no longer pending (Status: {Status}). Skipping.",
                    orderId, order.Status);
                return;
            }

            // Mark order as expired
            order.Status = OrderStatus.Expired;
            await _orderRepository.UpdateAsync(order);

            // Get lottery number IDs from order details
            var lotteryNumberIds = order.OrderDetails
                .Where(od => od.LotteryNumberId.HasValue)
                .Select(od => od.LotteryNumberId!.Value)
                .ToList();

            // Publish OrderExpiredEvent to release reserved numbers in Lottery Service
            await _eventBus.Publish(new OrderExpiredEvent
            {
                OrderId = order.OrderGuid,
                LotteryId = order.LotteryId,
                LotteryNumberIds = lotteryNumberIds
            });

            _logger.LogInformation(
                "OrderTimeoutJob: Order {OrderId} expired. Published OrderExpiredEvent to release {Count} numbers.",
                orderId, lotteryNumberIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OrderTimeoutJob: Error processing timeout for Order {OrderId}", orderId);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    /// <summary>
    /// Creates a unique job key for a specific order
    /// </summary>
    public static JobKey CreateJobKey(Guid orderId) => 
        new($"order-timeout-{orderId}", "order-expiration");
    
    /// <summary>
    /// Creates a unique trigger key for a specific order
    /// </summary>
    public static TriggerKey CreateTriggerKey(Guid orderId) => 
        new($"order-timeout-trigger-{orderId}", "order-expiration");
}
