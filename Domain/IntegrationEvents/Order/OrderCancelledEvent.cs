using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Integration event published when an order is cancelled by user.
/// Consumed by: Lottery microservice (to release reserved numbers back to available)
/// </summary>
public class OrderCancelledEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public string Reason { get; set; } = null!;
}

