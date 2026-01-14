using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Domain.Models;

public class LotteryNumber : BaseEntity
{
    public Guid Id { get; set; }
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public bool IsAvailable { get; set; }
    public NumberStatus Status { get; set; }
    
    /// <summary>
    /// Order ID that reserved this number (during checkout)
    /// </summary>
    public Guid? OrderId { get; set; }
    
    /// <summary>
    /// Ticket ID that purchased this number (after payment)
    /// </summary>
    public Guid? TicketId { get; set; }
    
    /// <summary>
    /// When the reservation expires (for pending orders)
    /// </summary>
    public DateTime? ReservationExpiresAt { get; set; }

    // Navigation
    public virtual LotteryDraw Lottery { get; set; } = null!;
}