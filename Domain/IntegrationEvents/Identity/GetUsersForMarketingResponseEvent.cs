using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Event published by Identity service in response to GetUsersForMarketingRequestEvent.
/// Contains the list of users and the original lottery information for the marketing campaign.
/// </summary>
public class GetUsersForMarketingResponseEvent : Event
{
    /// <summary>
    /// Correlation ID to match with the original request (Guid type for request-response pattern)
    /// </summary>
    public new Guid CorrelationId { get; set; }
    
    /// <summary>
    /// List of users for the marketing campaign
    /// </summary>
    public List<MarketingUserInfo> Users { get; set; } = [];
    
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the request failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    // Lottery information passed through for the marketing campaign
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public string LotteryDescription { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxTickets { get; set; }
}

/// <summary>
/// User information for marketing campaigns
/// </summary>
public class MarketingUserInfo
{
    public Guid UserGuid { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
