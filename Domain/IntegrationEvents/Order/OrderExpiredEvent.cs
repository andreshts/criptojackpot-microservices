using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Integration event published when an order expires (5 min timeout).
/// Consumed by: Lottery microservice (to release reserved numbers back to available)
/// </summary>
public class OrderExpiredEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid LotteryId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
}

