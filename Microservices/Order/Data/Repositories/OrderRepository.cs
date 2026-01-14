using CryptoJackpot.Order.Data.Context;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Order.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Models.Order> CreateAsync(Domain.Models.Order order)
    {
        var now = DateTime.UtcNow;
        order.CreatedAt = now;
        order.UpdatedAt = now;

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<Domain.Models.Order?> GetByIdAsync(Guid orderId)
        => await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderGuid == orderId);

    public async Task<Domain.Models.Order?> GetByIdWithTrackingAsync(Guid orderId)
        => await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderGuid == orderId);

    public async Task<IEnumerable<Domain.Models.Order>> GetByUserIdAsync(long userId)
        => await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Domain.Models.Order>> GetExpiredPendingOrdersAsync()
        => await _context.Orders
            .Where(o => o.Status == OrderStatus.Pending && o.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

    public async Task<Domain.Models.Order> UpdateAsync(Domain.Models.Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}

