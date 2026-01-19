using CryptoJackpot.Order.Data.Context;
using CryptoJackpot.Order.Domain.Interfaces;
using CryptoJackpot.Order.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Order.Data.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly OrderDbContext _context;

    public TicketRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        var now = DateTime.UtcNow;
        ticket.CreatedAt = now;
        ticket.UpdatedAt = now;

        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        return ticket;
    }

    public async Task<Ticket?> GetByGuidAsync(Guid ticketGuid)
        => await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TicketGuid == ticketGuid);

    public async Task<IEnumerable<Ticket>> GetByUserIdAsync(long userId)
        => await _context.Tickets
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByLotteryIdAsync(Guid lotteryId)
        => await _context.Tickets
            .AsNoTracking()
            .Where(t => t.LotteryId == lotteryId)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByOrderIdAsync(long orderId)
        => await _context.Tickets
            .AsNoTracking()
            .Where(t => t.OrderDetail.OrderId == orderId)
            .ToListAsync();

    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }
}

