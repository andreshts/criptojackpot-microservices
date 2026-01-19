using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Consumers;

/// <summary>
/// Consumes OrderTimeoutEvent after 5 minutes to check if order is still pending.
/// If order is still pending, marks it as expired and publishes OrderExpiredEvent.
/// </summary>
public class OrderTimeoutConsumer : IConsumer<OrderTimeoutEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderTimeoutConsumer> _logger;

    public OrderTimeoutConsumer(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        ILogger<OrderTimeoutConsumer> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderTimeoutEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderTimeoutEvent for Order {OrderId}. Checking if still pending...",
            message.OrderId);

        var order = await _orderRepository.GetByGuidWithTrackingAsync(message.OrderId);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for timeout processing", message.OrderId);
            return;
        }

        // Only process if order is still pending
        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogInformation(
                "Order {OrderId} is no longer pending (Status: {Status}). Skipping timeout.",
                message.OrderId, order.Status);
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
            "Order {OrderId} expired after timeout. Published OrderExpiredEvent to release {Count} numbers.",
            message.OrderId, lotteryNumberIds.Count);
    }
}

