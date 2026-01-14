using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Application.DTOs;

public class OrderDto
{
    public Guid OrderGuid { get; set; }
    public long UserId { get; set; }
    public Guid LotteryId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int[] SelectedNumbers { get; set; } = [];
    public int Series { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
    public Guid? TicketId { get; set; }
    public int SecondsRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
}

