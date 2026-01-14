using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public long UserId { get; set; }
    public Guid LotteryId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public int[] SelectedNumbers { get; set; } = [];
    public int Series { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
}

