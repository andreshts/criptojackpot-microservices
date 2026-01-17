using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
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
            TotalAmount = message.TotalAmount,
            Status = OrderStatus.Pending,
            ExpiresAt = message.ExpiresAt,
            SelectedNumbers = message.Numbers,
            Series = message.SeriesArray.FirstOrDefault(),
            LotteryNumberIds = message.LotteryNumberIds,
            IsGift = false,
            GiftRecipientId = null
        };

        var createdOrder = await _orderRepository.CreateAsync(order);

        // Publish OrderCreatedEvent to notify Lottery service (for backward compatibility)
        await _eventBus.Publish(new OrderCreatedEvent
        {
            OrderId = createdOrder.OrderGuid,
            LotteryId = createdOrder.LotteryId,
            UserId = createdOrder.UserId,
            LotteryNumberIds = createdOrder.LotteryNumberIds,
            ExpiresAt = createdOrder.ExpiresAt
        });

        // Schedule timeout event
        await _messageScheduler.SchedulePublish(
            message.ExpiresAt,
            new OrderTimeoutEvent
            {
                OrderId = createdOrder.OrderGuid,
                LotteryId = createdOrder.LotteryId,
                LotteryNumberIds = createdOrder.LotteryNumberIds
            });

        _logger.LogInformation(
            "Created order {OrderId} from NumbersReservedEvent. Amount: {Amount}, Expires: {ExpiresAt}",
            createdOrder.OrderGuid, createdOrder.TotalAmount, createdOrder.ExpiresAt);
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

        // Add numbers to existing order
        existingOrder.LotteryNumberIds.AddRange(message.LotteryNumberIds);
        existingOrder.SelectedNumbers = existingOrder.SelectedNumbers.Concat(message.Numbers).ToArray();
        existingOrder.TotalAmount += message.TotalAmount;
        
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
            LotteryNumberIds = message.LotteryNumberIds, // Only the new numbers
            ExpiresAt = existingOrder.ExpiresAt
        });

        _logger.LogInformation(
            "Added {Count} numbers to existing order {OrderId}. New total: {Amount}",
            message.LotteryNumberIds.Count, existingOrder.OrderGuid, existingOrder.TotalAmount);
    }
}
