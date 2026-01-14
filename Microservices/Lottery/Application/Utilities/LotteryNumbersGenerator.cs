using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Models;

namespace CryptoJackpot.Lottery.Application.Utilities;

/// <summary>
/// Utility class for generating lottery number combinations.
/// </summary>
public static class LotteryNumbersGenerator
{
    /// <summary>
    /// Generates all lottery number combinations using nested loops.
    /// Outer loop: Series (1 to TotalSeries) - Series 1 represents "01", Series 2 represents "02", etc.
    /// Inner loop: Numbers (MinNumber to MaxNumber, typically 00-99)
    /// </summary>
    /// <param name="lotteryId">The lottery ID to associate numbers with</param>
    /// <param name="minNumber">Minimum number in range (typically 0)</param>
    /// <param name="maxNumber">Maximum number in range (typically 99)</param>
    /// <param name="totalSeries">Total number of series to generate</param>
    /// <returns>Lazy enumerable of LotteryNumber entities</returns>
    public static IEnumerable<LotteryNumber> Generate(
        Guid lotteryId,
        int minNumber,
        int maxNumber,
        int totalSeries)
    {
        var now = DateTime.UtcNow;

        for (var series = 1; series <= totalSeries; series++)
        {
            for (var number = minNumber; number <= maxNumber; number++)
            {
                yield return new LotteryNumber
                {
                    Id = Guid.NewGuid(),
                    LotteryId = lotteryId,
                    Number = number,
                    Series = series,
                    Status = NumberStatus.Available,
                    TicketId = null,
                    OrderId = null,
                    ReservationExpiresAt = null,
                    CreatedAt = now,
                    UpdatedAt = now
                };
            }
        }
    }

    /// <summary>
    /// Calculates the total number of lottery numbers that will be generated.
    /// </summary>
    public static int CalculateTotalNumbers(int minNumber, int maxNumber, int totalSeries)
        => (maxNumber - minNumber + 1) * totalSeries;
}

