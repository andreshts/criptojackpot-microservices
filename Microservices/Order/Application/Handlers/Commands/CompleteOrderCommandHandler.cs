using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using CryptoJackpot.Order.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<TicketDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CompleteOrderCommandHandler> _logger;

    public CompleteOrderCommandHandler(
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        IMapper mapper,
        ILogger<CompleteOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TicketDto>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithTrackingAsync(request.OrderId);

        if (order is null)
            return Result.Fail<TicketDto>(new NotFoundError("Order not found"));

        // Verify the order belongs to the user
        if (order.UserId != request.UserId)
            return Result.Fail<TicketDto>(new ForbiddenError("You don't have permission to complete this order"));

        // Verify the order is in Pending status
        if (order.Status != OrderStatus.Pending)
            return Result.Fail<TicketDto>(new BadRequestError($"Order cannot be completed. Current status: {order.Status}"));

        // Verify the order hasn't expired
        if (order.ExpiresAt < DateTime.UtcNow)
        {
            order.Status = OrderStatus.Expired;
            await _orderRepository.UpdateAsync(order);
            
            _logger.LogWarning("Order {OrderId} has expired", request.OrderId);
            return Result.Fail<TicketDto>(new BadRequestError("Order has expired. Please create a new order."));
        }

        try
        {
            var now = DateTime.UtcNow;

            // Create the ticket (confirmed purchase)
            var ticket = new Ticket
            {
                TicketGuid = Guid.NewGuid(),
                OrderId = order.OrderGuid,
                LotteryId = order.LotteryId,
                UserId = order.UserId,
                PurchaseAmount = order.TotalAmount,
                PurchaseDate = now,
                Status = TicketStatus.Active,
                TransactionId = request.TransactionId,
                SelectedNumbers = order.SelectedNumbers,
                Series = order.Series,
                LotteryNumberIds = order.LotteryNumberIds,
                IsGift = order.IsGift,
                GiftRecipientId = order.GiftRecipientId
            };

            var createdTicket = await _ticketRepository.CreateAsync(ticket);

            // Update order to completed
            order.Status = OrderStatus.Completed;
            order.TicketId = createdTicket.TicketGuid;
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} completed. Ticket {TicketId} created for user {UserId}. Transaction: {TransactionId}",
                order.OrderGuid, createdTicket.TicketGuid, request.UserId, request.TransactionId);

            return Result.Ok(_mapper.Map<TicketDto>(createdTicket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete order {OrderId}", request.OrderId);
            return Result.Fail<TicketDto>(new InternalServerError("Failed to complete order"));
        }
    }
}

