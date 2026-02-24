using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;

/// <summary>
/// Integration event published when numbers are reserved via SignalR Hub.
/// Consumed by: Order microservice (to create/update pending order)
/// </summary>
public class NumbersReservedEvent : Event
{
    /// <summary>
    /// Pre-generated OrderId to use (allows immediate response to client)
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// The lottery ID these numbers belong to
    /// </summary>
    public Guid LotteryId { get; set; }
    
    /// <summary>
    /// The user who reserved the numbers
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// The reserved lottery number IDs
    /// </summary>
    public List<Guid> LotteryNumberIds { get; set; } = [];
    
    /// <summary>
    /// The actual numbers reserved (e.g., [10, 10] for number 10 series 1 and 2)
    /// </summary>
    public int[] Numbers { get; set; } = [];
    
    /// <summary>
    /// The series of each reserved number
    /// </summary>
    public int[] SeriesArray { get; set; } = [];
    
    /// <summary>
    /// Price per ticket
    /// </summary>
    public decimal TicketPrice { get; set; }
    
    /// <summary>
    /// Total amount for this reservation
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// CoinPayments currency ID for payment (e.g. "2")
    /// </summary>
    public string CryptoCurrencyId { get; set; } = null!;
    
    /// <summary>
    /// Crypto ticker symbol for payment (e.g. "LTCT", "BTC")
    /// </summary>
    public string CryptoCurrencySymbol { get; set; } = null!;
    
    /// <summary>
    /// When the reservation expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// If true, update existing pending order. If false, create new order.
    /// </summary>
    public bool IsAddToExistingOrder { get; set; }
    
    /// <summary>
    /// Existing order ID to update (if IsAddToExistingOrder is true)
    /// </summary>
    public Guid? ExistingOrderId { get; set; }
}
