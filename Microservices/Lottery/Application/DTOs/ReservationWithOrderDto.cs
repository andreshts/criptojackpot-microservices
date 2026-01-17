namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// DTO for the response when numbers are reserved via SignalR Hub.
/// Contains reservation details plus order information.
/// </summary>
public class ReservationWithOrderDto
{
    /// <summary>
    /// The order ID that was created/updated for this reservation.
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// The lottery ID these numbers belong to.
    /// </summary>
    public Guid LotteryId { get; set; }
    
    /// <summary>
    /// Total amount for the current order.
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Price per ticket.
    /// </summary>
    public decimal TicketPrice { get; set; }
    
    /// <summary>
    /// When the order/reservation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Seconds remaining until expiration.
    /// </summary>
    public int SecondsRemaining { get; set; }
    
    /// <summary>
    /// The reservations included in this response.
    /// </summary>
    public List<NumberReservationDto> Reservations { get; set; } = [];
    
    /// <summary>
    /// Whether this reservation was added to an existing order.
    /// </summary>
    public bool AddedToExistingOrder { get; set; }
}
