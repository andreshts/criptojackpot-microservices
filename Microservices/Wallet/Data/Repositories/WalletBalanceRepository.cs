using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Wallet.Data.Repositories;

public class WalletBalanceRepository : IWalletBalanceRepository
{
    private readonly WalletDbContext _context;

    public WalletBalanceRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<WalletBalance?> GetByUserAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        return await _context.WalletBalances
            .FirstOrDefaultAsync(b => b.UserGuid == userGuid, cancellationToken);
    }

    public async Task<WalletBalance> AddAsync(WalletBalance balance, CancellationToken cancellationToken = default)
    {
        var entry = await _context.WalletBalances.AddAsync(balance, cancellationToken);
        return entry.Entity;
    }

    public void Update(WalletBalance balance)
    {
        _context.WalletBalances.Update(balance);
    }
}

