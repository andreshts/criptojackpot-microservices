using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Data.Context;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace CryptoJackpot.Lottery.Data.Repositories;

public class PrizeRepository : IPrizeRepository
{
    private readonly LotteryDbContext _context;

    public PrizeRepository(LotteryDbContext context)
    {
        _context = context;
    }

    public async Task<Prize> CreatePrizeAsync(Prize prize)
    {
        var today = DateTime.UtcNow;
        prize.CreatedAt = today;
        prize.UpdatedAt = today;

        await _context.Prizes.AddAsync(prize);
        await _context.SaveChangesAsync();
        return prize;
    }

    public async Task<Prize?> GetPrizeAsync(Guid id)
        => await _context.Prizes.FindAsync(id);

    public async Task<PagedList<Prize>> GetAllPrizesAsync(Pagination pagination)
    {
        var query = _context.Prizes.Where(p => p.LotteryId == null);
        var totalItems = await query.CountAsync();

        var prizes = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedList<Prize>
        {
            Items = prizes,
            TotalItems = totalItems,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }
    
    public async Task<Prize> UpdatePrizeAsync(Prize prize)
    {
        prize.UpdatedAt = DateTime.UtcNow;
        _context.Prizes.Update(prize);
        await _context.SaveChangesAsync();
        return prize;
    }

    public async Task<Prize> DeletePrizeAsync(Prize prize)
    {
        prize.UpdatedAt = DateTime.UtcNow;
        prize.DeletedAt = DateTime.UtcNow;
        _context.Prizes.Update(prize);
        await _context.SaveChangesAsync();
        return prize;
    }

    public async Task LinkPrizeToLotteryAsync(Guid prizeId, Guid lotteryId)
    {
        var prize = await _context.Prizes.FindAsync(prizeId);
        if (prize is not null)
        {
            prize.LotteryId = lotteryId;
            prize.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnlinkPrizesFromLotteryAsync(Guid lotteryId)
    {
        var prizes = await _context.Prizes
            .Where(p => p.LotteryId == lotteryId)
            .ToListAsync();

        foreach (var prize in prizes)
        {
            prize.LotteryId = null;
            prize.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}