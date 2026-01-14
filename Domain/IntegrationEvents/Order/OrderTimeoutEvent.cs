using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Scheduled event that fires after 5 minutes to check if order is still pending.
/// If pending, triggers order expiration and releases reserved numbers.
/// </summary>
public class OrderTimeoutEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid LotteryId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
}

