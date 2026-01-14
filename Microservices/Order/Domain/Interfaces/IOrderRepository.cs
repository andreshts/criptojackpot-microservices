namespace CryptoJackpot.Order.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Models.Order> CreateAsync(Models.Order order);
    Task<Models.Order?> GetByIdAsync(Guid orderId);
    Task<Models.Order?> GetByIdWithTrackingAsync(Guid orderId);
    Task<IEnumerable<Models.Order>> GetByUserIdAsync(long userId);
    Task<IEnumerable<Models.Order>> GetExpiredPendingOrdersAsync();
    Task<Models.Order> UpdateAsync(Models.Order order);
    Task SaveChangesAsync();
}

