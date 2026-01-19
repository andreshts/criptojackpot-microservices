using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Representa un boleto de lotería comprado (pago confirmado).
/// Se genera un ticket por cada OrderDetail después del pago exitoso.
/// </summary>
public class Ticket : BaseEntity
{
    /// <summary>
    /// External GUID for API exposure and cross-service communication
    /// </summary>
    public Guid TicketGuid { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Foreign key to the OrderDetail that generated this ticket
    /// </summary>
    public long OrderDetailId { get; set; }
    
    public Guid LotteryId { get; set; }
    
    /// <summary>
    /// Owner of the ticket (can be different from buyer if it's a gift)
    /// </summary>
    public long UserId { get; set; }

    public decimal PurchaseAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public TicketStatus Status { get; set; }
    public string TransactionId { get; set; } = null!;
    
    /// <summary>
    /// The lottery number for this ticket
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// The series for this ticket
    /// </summary>
    public int Series { get; set; }
    
    /// <summary>
    /// ID of purchased lottery number from Lottery microservice
    /// </summary>
    public Guid? LotteryNumberId { get; set; }
    
    /// <summary>
    /// Is this ticket a gift?
    /// </summary>
    public bool IsGift { get; set; }
    
    /// <summary>
    /// User ID of the original buyer (if IsGift is true, this is different from UserId)
    /// </summary>
    public long? GiftSenderId { get; set; }
    
    /// <summary>
    /// IDs of won prizes (if ticket won)
    /// </summary>
    public List<long>? WonPrizeIds { get; set; }

    // Navigation
    public virtual OrderDetail OrderDetail { get; set; } = null!;
}