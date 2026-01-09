using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Domain.Models;
namespace CryptoJackpot.Lottery.Domain.Interfaces;

public interface IPrizeRepository
{
    Task<Prize> CreatePrizeAsync(Prize prize);
    Task<Prize?> GetPrizeAsync(Guid id);
    Task<PagedList<Prize>> GetAllPrizesAsync(Pagination pagination);
    Task<Prize> UpdatePrizeAsync(Prize prize);
    Task<Prize> DeletePrizeAsync(Prize prize);
    Task LinkPrizeToLotteryAsync(Guid prizeId, Guid lotteryId);
    Task UnlinkPrizesFromLotteryAsync(Guid lotteryId);
}