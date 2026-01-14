using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly IMessageScheduler _messageScheduler;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private const int OrderExpirationMinutes = 5;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        IMessageScheduler messageScheduler,
        IMapper mapper,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _messageScheduler = messageScheduler;
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
                TotalAmount = request.TotalAmount,
                Status = OrderStatus.Pending,
                ExpiresAt = expiresAt,
                SelectedNumbers = request.SelectedNumbers,
                Series = request.Series,
                LotteryNumberIds = request.LotteryNumberIds,
                IsGift = request.IsGift,
                GiftRecipientId = request.GiftRecipientId
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Publish event to notify Lottery Service about the order (reserve numbers)
            await _eventBus.Publish(new OrderCreatedEvent
            {
                OrderId = createdOrder.OrderGuid,
                LotteryId = createdOrder.LotteryId,
                UserId = createdOrder.UserId,
                LotteryNumberIds = createdOrder.LotteryNumberIds,
                ExpiresAt = createdOrder.ExpiresAt
            });

            // Schedule timeout event to fire after 5 minutes
            await _messageScheduler.SchedulePublish(
                expiresAt,
                new OrderTimeoutEvent
                {
                    OrderId = createdOrder.OrderGuid,
                    LotteryId = createdOrder.LotteryId,
                    LotteryNumberIds = createdOrder.LotteryNumberIds
                },
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

