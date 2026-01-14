using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Services;

/// <summary>
/// Service for lottery number operations.
/// Handles real-time number reservations for SignalR hub.
/// </summary>
public class LotteryNumberService : ILotteryNumberService
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILogger<LotteryNumberService> _logger;
    private const int ReservationMinutes = 5;

    public LotteryNumberService(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILogger<LotteryNumberService> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _logger = logger;
    }

    public async Task<List<AvailableNumberDto>> GetAvailableNumbersAsync(Guid lotteryId)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByIdAsync(lotteryId);
        if (lottery == null)
            return [];

        var numbers = await _lotteryNumberRepository.GetNumbersByLotteryAsync(lotteryId);
        
        // Group by number and count available series
        var grouped = numbers
            .GroupBy(n => n.Number)
            .Select(g => new AvailableNumberDto
            {
                Number = g.Key,
                AvailableSeries = g.Count(n => n.Status == NumberStatus.Available),
                TotalSeries = g.Count()
            })
            .OrderBy(n => n.Number)
            .ToList();

        return grouped;
    }

    public async Task<Result<NumberReservationDto>> ReserveNumberAsync(
        Guid lotteryId, 
        int number, 
        int? series, 
        long userId)
    {
        // Find available number (first available series if not specified)
        var availableNumber = await _lotteryNumberRepository.FindAvailableNumberAsync(
            lotteryId, number, series);

        if (availableNumber == null)
        {
            var message = series.HasValue
                ? $"Number {number} series {series} is not available"
                : $"Number {number} is not available in any series";
            
            _logger.LogWarning(
                "Failed to reserve number {Number} series {Series} in lottery {LotteryId}: not available",
                number, series, lotteryId);
            
            return Result.Fail<NumberReservationDto>(message);
        }

        // Reserve the number
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(ReservationMinutes);

        availableNumber.Status = NumberStatus.Reserved;
        availableNumber.ReservationExpiresAt = expiresAt;
        availableNumber.UpdatedAt = now;

        await _lotteryNumberRepository.UpdateAsync(availableNumber);

        _logger.LogInformation(
            "Number {Number} series {Series} reserved for user {UserId} in lottery {LotteryId}. Expires at {ExpiresAt}",
            availableNumber.Number, availableNumber.Series, userId, lotteryId, expiresAt);

        return Result.Ok(new NumberReservationDto
        {
            NumberId = availableNumber.Id,
            LotteryId = availableNumber.LotteryId,
            Number = availableNumber.Number,
            Series = availableNumber.Series,
            ReservationExpiresAt = expiresAt,
            SecondsRemaining = ReservationMinutes * 60
        });
    }

    public async Task<List<NumberStatusDto>> GetNumberStatusesAsync(Guid lotteryId)
    {
        var numbers = await _lotteryNumberRepository.GetNumbersByLotteryAsync(lotteryId);
        
        return numbers.Select(n => new NumberStatusDto
        {
            NumberId = n.Id,
            Number = n.Number,
            Series = n.Series,
            Status = n.Status
        }).ToList();
    }
}

