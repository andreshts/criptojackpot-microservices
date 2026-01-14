using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Lottery.Application.Interfaces;

/// <summary>
/// Service interface for lottery number operations.
/// Used by SignalR Hub for real-time number management.
/// </summary>
public interface ILotteryNumberService
{
    /// <summary>
    /// Gets available numbers for a lottery (grouped by number with series count).
    /// </summary>
    Task<List<AvailableNumberDto>> GetAvailableNumbersAsync(Guid lotteryId);

    /// <summary>
    /// Reserves a specific number for a user.
    /// If series is null, reserves the first available series for that number.
    /// </summary>
    Task<Result<NumberReservationDto>> ReserveNumberAsync(Guid lotteryId, int number, int? series, long userId);

    /// <summary>
    /// Gets detailed number status for a lottery.
    /// </summary>
    Task<List<NumberStatusDto>> GetNumberStatusesAsync(Guid lotteryId);
}

