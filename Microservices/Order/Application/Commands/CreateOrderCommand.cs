using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public long UserId { get; set; }
    public Guid LotteryId { get; set; }
    public List<CreateOrderItemCommand> Items { get; set; } = [];
}

public class CreateOrderItemCommand
{
    public int Number { get; set; }
    public int Series { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public Guid? LotteryNumberId { get; set; }
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
}

