using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderCancelledEvent to release reserved lottery numbers back to available.
/// Broadcasts the release via SignalR to all connected clients.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryNotificationService _notificationService;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryNotificationService notificationService,
        ILogger<OrderCancelledConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderCancelledEvent for Order {OrderId}. Reason: {Reason}. Releasing {Count} reserved numbers.",
            message.OrderId, message.Reason, message.LotteryNumberIds.Count);

        // Get the numbers before releasing to get their details
        var numbers = await _lotteryNumberRepository.GetByIdsAsync(message.LotteryNumberIds);
        
        var success = await _lotteryNumberRepository.ReleaseNumbersByOrderAsync(message.OrderId);

        if (success)
        {
            // Broadcast via SignalR
            var releasedNumbers = numbers.Select(n => new NumberStatusDto
            {
                NumberId = n.Id,
                Number = n.Number,
                Series = n.Series,
                Status = NumberStatus.Available
            }).ToList();

            await _notificationService.NotifyNumbersReleasedAsync(message.LotteryId, releasedNumbers);

            _logger.LogInformation(
                "Successfully released and broadcasted {Count} numbers for cancelled Order {OrderId}",
                releasedNumbers.Count, message.OrderId);
        }
        else
        {
            _logger.LogWarning(
                "No reserved numbers found to release for Order {OrderId}",
                message.OrderId);
        }
    }
}

