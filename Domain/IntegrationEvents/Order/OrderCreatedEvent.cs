using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Integration event published when an order is created.
/// Consumed by: Lottery microservice (to confirm number reservation)
/// </summary>
public class OrderCreatedEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
}

