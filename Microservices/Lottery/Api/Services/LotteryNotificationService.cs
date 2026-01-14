using CryptoJackpot.Lottery.Api.Hubs;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CryptoJackpot.Lottery.Api.Services;

/// <summary>
/// Service for broadcasting lottery updates via SignalR.
/// Implemented in the API layer because it needs access to the concrete Hub type.
/// </summary>
public class LotteryNotificationService : ILotteryNotificationService 
{
    private readonly IHubContext<LotteryHub, ILotteryHubClient> _hubContext;
    private readonly ILogger<LotteryNotificationService> _logger;

    public LotteryNotificationService(
        IHubContext<LotteryHub, ILotteryHubClient> hubContext,
        ILogger<LotteryNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNumbersReleasedAsync(Guid lotteryId, List<NumberStatusDto> numbers)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        
        await _hubContext.Clients.Group(groupName).NumbersReleased(lotteryId, numbers);
        
        _logger.LogInformation(
            "Broadcasted {Count} numbers released for lottery {LotteryId}",
            numbers.Count, lotteryId);
    }

    public async Task NotifyNumbersSoldAsync(Guid lotteryId, List<NumberStatusDto> numbers)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        
        await _hubContext.Clients.Group(groupName).NumbersSold(lotteryId, numbers);
        
        _logger.LogInformation(
            "Broadcasted {Count} numbers sold for lottery {LotteryId}",
            numbers.Count, lotteryId);
    }

    public async Task NotifyNumberReservedAsync(Guid lotteryId, Guid numberId, int number, int series)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        
        await _hubContext.Clients.Group(groupName).NumberReserved(lotteryId, numberId, number, series);
        
        _logger.LogInformation(
            "Broadcasted number {Number} series {Series} reserved for lottery {LotteryId}",
            number, series, lotteryId);
    }

    private static string GetLotteryGroupName(Guid lotteryId) => $"lottery-{lotteryId}";
}

