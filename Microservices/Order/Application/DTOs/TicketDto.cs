using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Application.DTOs;

public class TicketDto
{
    public Guid TicketGuid { get; set; }
    public Guid OrderId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    public decimal PurchaseAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public TicketStatus Status { get; set; }
    public string TransactionId { get; set; } = null!;
    public int[] SelectedNumbers { get; set; } = [];
    public int Series { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
    public DateTime CreatedAt { get; set; }
}

