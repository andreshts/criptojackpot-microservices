namespace CryptoJackpot.Order.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Models.Order> CreateAsync(Models.Order order);
    Task<Models.Order?> GetByGuidAsync(Guid orderGuid);
    Task<Models.Order?> GetByGuidWithTrackingAsync(Guid orderGuid);
    Task<IEnumerable<Models.Order>> GetByUserIdAsync(long userId);
    Task<IEnumerable<Models.Order>> GetExpiredPendingOrdersAsync();
    Task<List<Models.Order>> GetExpiredPendingOrdersAsync(DateTime cutoffTime, CancellationToken cancellationToken = default);
    Task<Models.Order> UpdateAsync(Models.Order order);
    Task SaveChangesAsync();
}

