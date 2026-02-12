using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Repositories;

public class RecoveryCodeRepository : IRecoveryCodeRepository
{
    private readonly IdentityDbContext _context;

    public RecoveryCodeRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserRecoveryCode>> GetUnusedByUserIdAsync(long userId)
    {
        return await _context.UserRecoveryCodes
            .Where(c => c.UserId == userId && !c.IsUsed)
            .ToListAsync();
    }

    public async Task<int> GetRemainingCountAsync(long userId)
    {
        return await _context.UserRecoveryCodes
            .CountAsync(c => c.UserId == userId && !c.IsUsed);
    }

    public async Task AddRangeAsync(IEnumerable<UserRecoveryCode> codes)
    {
        await _context.UserRecoveryCodes.AddRangeAsync(codes);
    }

    public async Task DeleteAllByUserIdAsync(long userId)
    {
        await _context.UserRecoveryCodes
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync();
    }
}

