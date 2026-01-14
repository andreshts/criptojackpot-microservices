using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

public class CancelOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; set; }
    public long UserId { get; set; }
    public string Reason { get; set; } = "User cancelled";
}

