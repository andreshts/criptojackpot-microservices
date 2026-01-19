using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Representa un intento de compra / carrito.
/// Tiene un countdown de 5 minutos para completar el pago.
/// </summary>
public class Order : BaseEntity
{
    public long Id { get; set; }
    public Guid OrderGuid { get; set; }
    public long UserId { get; set; }
    public Guid LotteryId { get; set; }
    
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
    /// Total amount calculated from order details
    /// </summary>
    public decimal TotalAmount => OrderDetails.Sum(d => d.Subtotal);
    
    /// <summary>
    /// Total number of items in the order
    /// </summary>
    public int TotalItems => OrderDetails.Sum(d => d.Quantity);
    
    // Navigation properties
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

