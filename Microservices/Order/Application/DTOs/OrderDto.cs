using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Application.DTOs;

public class OrderDto
{
    public long Id { get; set; }
    public Guid OrderGuid { get; set; }
    public long UserId { get; set; }
    public Guid LotteryId { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int SecondsRemaining { get; set; }
    public List<OrderDetailDto> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class OrderDetailDto
{
    public long Id { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public Guid? LotteryNumberId { get; set; }
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
    public Guid? TicketId { get; set; }
}

