using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Integration event published when an order is completed (payment successful).
/// Consumed by: Lottery microservice (to mark numbers as sold permanently)
/// </summary>
public class OrderCompletedEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid TicketId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public string TransactionId { get; set; } = null!;
}

