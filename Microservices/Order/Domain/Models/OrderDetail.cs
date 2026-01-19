using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Representa una línea de detalle de una orden.
/// Cada detalle corresponde a una selección de números para la lotería.
/// </summary>
public class OrderDetail : BaseEntity
{
    public long Id { get; set; }
    
    /// <summary>
    /// Foreign key to the parent Order
    /// </summary>
    public long OrderId { get; set; }
    
    /// <summary>
    /// Unit price for this line item
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Quantity of tickets for this selection
    /// </summary>
    public int Quantity { get; set; } = 1;
    
    /// <summary>
    /// Subtotal for this line (UnitPrice * Quantity)
    /// </summary>
    public decimal Subtotal => UnitPrice * Quantity;
    
    /// <summary>
    /// The lottery number selected for this line item
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// Selected series for this line item
    /// </summary>
    public int Series { get; set; }
    
    /// <summary>
    /// ID of reserved lottery number from Lottery microservice
    /// </summary>
    public Guid? LotteryNumberId { get; set; }
    
    /// <summary>
    /// Is this line item a gift for another user?
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
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Ticket? Ticket { get; set; }
}