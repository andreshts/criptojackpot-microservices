using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithTrackingAsync(request.OrderId);

        if (order is null)
            return Result.Fail<OrderDto>(new NotFoundError("Order not found"));

        // Verify the order belongs to the user
        if (order.UserId != request.UserId)
            return Result.Fail<OrderDto>(new ForbiddenError("You don't have permission to cancel this order"));

        // Verify the order is in Pending status
        if (order.Status != OrderStatus.Pending)
            return Result.Fail<OrderDto>(new BadRequestError($"Order cannot be cancelled. Current status: {order.Status}"));

        try
        {
            // Update order to cancelled
            order.Status = OrderStatus.Cancelled;
            var updatedOrder = await _orderRepository.UpdateAsync(order);

            // Publish event to release reserved numbers
            await _eventBus.Publish(new OrderCancelledEvent
            {
                OrderId = order.OrderGuid,
                LotteryId = order.LotteryId,
                UserId = order.UserId,
                LotteryNumberIds = order.LotteryNumberIds,
                Reason = request.Reason
            });

            _logger.LogInformation(
                "Order {OrderId} cancelled by user {UserId}. Reason: {Reason}. Event published.",
                order.OrderGuid, request.UserId, request.Reason);

            return Result.Ok(_mapper.Map<OrderDto>(updatedOrder));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order {OrderId}", request.OrderId);
            return Result.Fail<OrderDto>(new InternalServerError("Failed to cancel order"));
        }
    }
}

