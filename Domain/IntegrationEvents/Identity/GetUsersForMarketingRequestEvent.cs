using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Event published by Notification service to request users for marketing campaign.
/// Identity service will respond with GetUsersForMarketingResponseEvent.
/// </summary>
public class GetUsersForMarketingRequestEvent : Event
{
    /// <summary>
    /// Correlation ID to match request with response (Guid type for request-response pattern)
    /// </summary>
    public new Guid CorrelationId { get; set; }
    
    /// <summary>
    /// Lottery information to include in the response for the marketing campaign
    /// </summary>
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public string LotteryDescription { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxTickets { get; set; }
    
    /// <summary>
    /// If true, only returns active users (Status = true means email confirmed)
    /// </summary>
    public bool OnlyActiveUsers { get; set; } = true;
}
