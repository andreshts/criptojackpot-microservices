using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.Interfaces;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly IOrderTimeoutScheduler _orderTimeoutScheduler;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private const int OrderExpirationMinutes = 5;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        IOrderTimeoutScheduler orderTimeoutScheduler,
        IMapper mapper,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _orderTimeoutScheduler = orderTimeoutScheduler;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(OrderExpirationMinutes);
            
            var order = new Domain.Models.Order
            {
                OrderGuid = Guid.NewGuid(),
                UserId = request.UserId,
                LotteryId = request.LotteryId,
                Status = OrderStatus.Pending,
                ExpiresAt = expiresAt
            };

            // Create order details from the request items
            foreach (var item in request.Items)
            {
                order.OrderDetails.Add(new Domain.Models.OrderDetail
                {
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Number = item.Number,
                    Series = item.Series,
                    LotteryNumberId = item.LotteryNumberId,
                    IsGift = item.IsGift,
                    GiftRecipientId = item.GiftRecipientId
                });
            }

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Get lottery number IDs for the event
            var lotteryNumberIds = createdOrder.OrderDetails
                .Where(od => od.LotteryNumberId.HasValue)
                .Select(od => od.LotteryNumberId!.Value)
                .ToList();

            // Publish event to notify Lottery Service about the order (reserve numbers)
            await _eventBus.Publish(new OrderCreatedEvent
            {
                OrderId = createdOrder.OrderGuid,
                LotteryId = createdOrder.LotteryId,
                UserId = createdOrder.UserId,
                LotteryNumberIds = lotteryNumberIds,
                ExpiresAt = createdOrder.ExpiresAt
            });

            // Schedule timeout using Quartz with database persistence
            await _orderTimeoutScheduler.ScheduleOrderTimeoutAsync(
                createdOrder.OrderGuid,
                createdOrder.LotteryId,
                lotteryNumberIds,
                createdOrder.ExpiresAt,
                cancellationToken);

            _logger.LogInformation(
                "Order {OrderId} created for user {UserId}. Timeout scheduled at {ExpiresAt}.",
                createdOrder.OrderGuid, request.UserId, createdOrder.ExpiresAt);

            var dto = _mapper.Map<OrderDto>(createdOrder);
            dto.SecondsRemaining = (int)(createdOrder.ExpiresAt - now).TotalSeconds;

            return ResultExtensions.Created(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for user {UserId}", request.UserId);
            return Result.Fail<OrderDto>(new InternalServerError("Failed to create order"));
        }
    }
}

