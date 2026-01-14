using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Representa un boleto de lotería comprado (pago confirmado).
/// </summary>
public class Ticket : BaseEntity
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
    
    /// <summary>
    /// IDs of purchased lottery numbers from Lottery microservice
    /// </summary>
    public List<Guid> LotteryNumberIds { get; set; } = [];
    
    /// <summary>
    /// Is this ticket a gift?
    /// </summary>
    public bool IsGift { get; set; }
    
    /// <summary>
    /// User ID of the gift recipient (if IsGift is true)
    /// </summary>
    public long? GiftRecipientId { get; set; }
    
    /// <summary>
    /// IDs of won prizes (if ticket won)
    /// </summary>
    public List<long>? WonPrizeIds { get; set; }

    // Navigation
    public virtual Order Order { get; set; } = null!;
}