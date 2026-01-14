using CryptoJackpot.Lottery.Domain.Models;
namespace CryptoJackpot.Lottery.Domain.Interfaces;

public interface ILotteryNumberRepository
{
    Task<IEnumerable<LotteryNumber>> GetNumbersByLotteryAsync(Guid lotteryId);
    Task<HashSet<int>> GetSoldNumbersAsync(Guid lotteryId);
    Task<bool> IsNumberAvailableAsync(Guid lotteryId, int number, int series);
    Task<List<int>> GetAlreadyReservedNumbersAsync(Guid lotteryId, int series, IEnumerable<int> numbers);
    Task<List<(int Number, int Series)>> GetRandomAvailableNumbersWithSeriesAsync(
        Guid lotteryId, int count, int maxNumber, int totalSeries, int minNumber = 1);
    Task<List<int>> GetRandomAvailableNumbersAsync(Guid lotteryId, int count, int maxNumber, int minNumber = 1);
    Task AddRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers);
    Task<bool> ReleaseNumbersByTicketAsync(Guid ticketId);
    
    // Order integration methods
    Task<bool> ReserveNumbersAsync(List<Guid> numberIds, Guid orderId);
    Task<bool> ConfirmNumbersSoldAsync(List<Guid> numberIds, Guid ticketId);
    Task<bool> ReleaseNumbersByOrderAsync(Guid orderId);
    Task<List<LotteryNumber>> GetByIdsAsync(List<Guid> numberIds);
    
    // SignalR/Real-time methods
    Task<LotteryNumber?> FindAvailableNumberAsync(Guid lotteryId, int number, int? series = null);
    Task<LotteryNumber> UpdateAsync(LotteryNumber lotteryNumber);
}