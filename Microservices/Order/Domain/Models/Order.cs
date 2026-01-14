using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Representa un intento de compra / carrito.
/// Tiene un countdown de 5 minutos para completar el pago.
/// </summary>
public class Order : BaseEntity
{
    public Guid OrderGuid { get; set; }
    public long UserId { get; set; }
    public Guid LotteryId { get; set; }
    
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Expiration time for pending orders (5 minutes from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Computed property to check if order is expired
    /// </summary>
    public bool IsExpired => Status == OrderStatus.Pending && DateTime.UtcNow > ExpiresAt;
    
    /// <summary>
    /// Selected numbers for purchase
    /// </summary>
    public int[] SelectedNumbers { get; set; } = [];
    
    /// <summary>
    /// Selected series
    /// </summary>
    public int Series { get; set; }
    
    /// <summary>
    /// IDs of reserved lottery numbers from Lottery microservice
    /// </summary>
    public List<Guid> LotteryNumberIds { get; set; } = [];
    
    /// <summary>
    /// Is this order a gift for another user?
    /// </summary>
    public bool IsGift { get; set; }
    
    /// <summary>
    /// User ID of the gift recipient (if IsGift is true)
    /// </summary>
    public long? GiftRecipientId { get; set; }
    
    /// <summary>
    /// The ticket created after successful payment (null if pending/expired)
    /// </summary>
    public Guid? TicketId { get; set; }
    
    // Navigation
    public virtual Ticket? Ticket { get; set; }
}

