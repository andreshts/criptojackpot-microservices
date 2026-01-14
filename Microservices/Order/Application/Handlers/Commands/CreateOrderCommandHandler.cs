using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private const int OrderExpirationMinutes = 5;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            
            var order = new Domain.Models.Order
            {
                OrderGuid = Guid.NewGuid(),
                UserId = request.UserId,
                LotteryId = request.LotteryId,
                TotalAmount = request.TotalAmount,
                Status = OrderStatus.Pending,
                ExpiresAt = now.AddMinutes(OrderExpirationMinutes),
                SelectedNumbers = request.SelectedNumbers,
                Series = request.Series,
                LotteryNumberIds = request.LotteryNumberIds,
                IsGift = request.IsGift,
                GiftRecipientId = request.GiftRecipientId
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} created for user {UserId}. Expires at {ExpiresAt}",
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

