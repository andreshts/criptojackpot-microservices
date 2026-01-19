using CryptoJackpot.Order.Domain.Models;

namespace CryptoJackpot.Order.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket> CreateAsync(Ticket ticket);
    Task<Ticket?> GetByGuidAsync(Guid ticketGuid);
    Task<IEnumerable<Ticket>> GetByUserIdAsync(long userId);
    Task<IEnumerable<Ticket>> GetByLotteryIdAsync(Guid lotteryId);
    Task<IEnumerable<Ticket>> GetByOrderIdAsync(long orderId);
    Task<Ticket> UpdateAsync(Ticket ticket);
}

