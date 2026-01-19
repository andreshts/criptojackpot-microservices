using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Application.DTOs;

public class TicketDto
{
    public long Id { get; set; }
    public Guid TicketGuid { get; set; }
    public long OrderDetailId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    public decimal PurchaseAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public TicketStatus Status { get; set; }
    public string TransactionId { get; set; } = null!;
    public int Number { get; set; }
    public int Series { get; set; }
    public Guid? LotteryNumberId { get; set; }
    public bool IsGift { get; set; }
    public long? GiftSenderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

