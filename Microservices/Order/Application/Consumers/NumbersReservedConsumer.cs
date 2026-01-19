using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using CryptoJackpot.Order.Domain.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Consumers;

/// <summary>
/// Consumes NumbersReservedEvent from Lottery microservice to create/update orders.
/// This enables automatic order creation when numbers are reserved via SignalR Hub.
/// </summary>
public class NumbersReservedConsumer : IConsumer<NumbersReservedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly IMessageScheduler _messageScheduler;
    private readonly ILogger<NumbersReservedConsumer> _logger;

    public NumbersReservedConsumer(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        IMessageScheduler messageScheduler,
        ILogger<NumbersReservedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _messageScheduler = messageScheduler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NumbersReservedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received NumbersReservedEvent. OrderId: {OrderId}, LotteryId: {LotteryId}, UserId: {UserId}, Count: {Count}",
            message.OrderId, message.LotteryId, message.UserId, message.LotteryNumberIds.Count);

        if (message.IsAddToExistingOrder && message.ExistingOrderId.HasValue)
        {
            await AddToExistingOrder(message);
        }
        else
        {
            await CreateNewOrder(message);
        }
    }

    private async Task CreateNewOrder(NumbersReservedEvent message)
    {
        // Check if order already exists (idempotency)
        var existingOrder = await _orderRepository.GetByIdAsync(message.OrderId);
        if (existingOrder != null)
        {
            _logger.LogInformation(
                "Order {OrderId} already exists. Skipping creation (idempotent).",
                message.OrderId);
            return;
        }

        var order = new Domain.Models.Order
        {
            OrderGuid = message.OrderId,
            UserId = message.UserId,
            LotteryId = message.LotteryId,
            Status = OrderStatus.Pending,
            ExpiresAt = message.ExpiresAt
        };

        // Create order details from the reserved numbers
        for (var i = 0; i < message.Numbers.Length; i++)
        {
            var series = i < message.SeriesArray.Length ? message.SeriesArray[i] : message.SeriesArray.FirstOrDefault();
            var lotteryNumberId = i < message.LotteryNumberIds.Count ? message.LotteryNumberIds[i] : (Guid?)null;
            var unitPrice = message.LotteryNumberIds.Count > 0 
                ? message.TotalAmount / message.LotteryNumberIds.Count 
                : message.TotalAmount;

            order.OrderDetails.Add(new OrderDetail
            {
                Number = message.Numbers[i],
                Series = series,
                UnitPrice = unitPrice,
                Quantity = 1,
                LotteryNumberId = lotteryNumberId,
                IsGift = false
            });
        }

        var createdOrder = await _orderRepository.CreateAsync(order);

        // Get lottery number IDs for the event
        var lotteryNumberIds = createdOrder.OrderDetails
            .Where(od => od.LotteryNumberId.HasValue)
            .Select(od => od.LotteryNumberId!.Value)
            .ToList();

        // Publish OrderCreatedEvent to notify Lottery service
        await _eventBus.Publish(new OrderCreatedEvent
        {
            OrderId = createdOrder.OrderGuid,
            LotteryId = createdOrder.LotteryId,
            UserId = createdOrder.UserId,
            LotteryNumberIds = lotteryNumberIds,
            ExpiresAt = createdOrder.ExpiresAt
        });

        // Schedule timeout event
        await _messageScheduler.SchedulePublish(
            message.ExpiresAt,
            new OrderTimeoutEvent
            {
                OrderId = createdOrder.OrderGuid,
                LotteryId = createdOrder.LotteryId,
                LotteryNumberIds = lotteryNumberIds
            });

        _logger.LogInformation(
            "Created order {OrderId} from NumbersReservedEvent. Items: {ItemCount}, Amount: {Amount}, Expires: {ExpiresAt}",
            createdOrder.OrderGuid, createdOrder.OrderDetails.Count, createdOrder.TotalAmount, createdOrder.ExpiresAt);
    }

    private async Task AddToExistingOrder(NumbersReservedEvent message)
    {
        var existingOrder = await _orderRepository.GetByIdWithTrackingAsync(message.ExistingOrderId!.Value);
        
        if (existingOrder == null)
        {
            _logger.LogWarning(
                "Existing order {OrderId} not found. Creating new order instead.",
                message.ExistingOrderId);
            await CreateNewOrder(message);
            return;
        }

        // Check if order is still pending
        if (existingOrder.Status != OrderStatus.Pending)
        {
            _logger.LogWarning(
                "Existing order {OrderId} is not pending (Status: {Status}). Creating new order instead.",
                message.ExistingOrderId, existingOrder.Status);
            await CreateNewOrder(message);
            return;
        }

        // Check if order is expired
        if (existingOrder.IsExpired)
        {
            _logger.LogWarning(
                "Existing order {OrderId} is expired. Creating new order instead.",
                message.ExistingOrderId);
            await CreateNewOrder(message);
            return;
        }

        // Add new order details for the reserved numbers
        for (var i = 0; i < message.Numbers.Length; i++)
        {
            var series = i < message.SeriesArray.Length ? message.SeriesArray[i] : message.SeriesArray.FirstOrDefault();
            var lotteryNumberId = i < message.LotteryNumberIds.Count ? message.LotteryNumberIds[i] : (Guid?)null;
            var unitPrice = message.LotteryNumberIds.Count > 0 
                ? message.TotalAmount / message.LotteryNumberIds.Count 
                : message.TotalAmount;

            existingOrder.OrderDetails.Add(new OrderDetail
            {
                Number = message.Numbers[i],
                Series = series,
                UnitPrice = unitPrice,
                Quantity = 1,
                LotteryNumberId = lotteryNumberId,
                IsGift = false
            });
        }
        
        // Extend expiration to match the new reservation
        if (message.ExpiresAt > existingOrder.ExpiresAt)
        {
            existingOrder.ExpiresAt = message.ExpiresAt;
        }

        await _orderRepository.UpdateAsync(existingOrder);

        // Publish OrderCreatedEvent for the new numbers (to confirm reservation in Lottery)
        await _eventBus.Publish(new OrderCreatedEvent
        {
            OrderId = existingOrder.OrderGuid,
            LotteryId = message.LotteryId,
            UserId = message.UserId,
            LotteryNumberIds = message.LotteryNumberIds,
            ExpiresAt = existingOrder.ExpiresAt
        });

        _logger.LogInformation(
            "Added {Count} items to existing order {OrderId}. New total: {Amount}",
            message.Numbers.Length, existingOrder.OrderGuid, existingOrder.TotalAmount);
    }
}
