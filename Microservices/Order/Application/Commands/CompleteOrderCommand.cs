using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

public class CompleteOrderCommand : IRequest<Result<TicketDto>>
{
    public Guid OrderId { get; set; }
    public long UserId { get; set; }
    public string TransactionId { get; set; } = null!;
}

