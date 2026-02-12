using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context;

    public RefreshTokenRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserRefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await _context.UserRefreshTokens
            .Include(t => t.User)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task<IReadOnlyList<UserRefreshToken>> GetActiveByUserIdAsync(long userId)
    {
        return await _context.UserRefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserRefreshToken>> GetByFamilyIdAsync(Guid familyId)
    {
        return await _context.UserRefreshTokens
            .Where(t => t.FamilyId == familyId)
            .ToListAsync();
    }

    public async Task AddAsync(UserRefreshToken token)
    {
        await _context.UserRefreshTokens.AddAsync(token);
    }

    public async Task RevokeAllByUserIdAsync(long userId, string reason)
    {
        var activeTokens = await _context.UserRefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.Revoke(reason);
        }
    }

    public async Task RevokeByFamilyIdAsync(Guid familyId, string reason)
    {
        var familyTokens = await _context.UserRefreshTokens
            .Where(t => t.FamilyId == familyId && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in familyTokens)
        {
            token.Revoke(reason);
        }
    }

    public async Task<int> DeleteExpiredAsync(int olderThanDays = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        
        return await _context.UserRefreshTokens
            .Where(t => t.ExpiresAt < cutoffDate || (t.IsRevoked && t.RevokedAt < cutoffDate))
            .ExecuteDeleteAsync();
    }
}

