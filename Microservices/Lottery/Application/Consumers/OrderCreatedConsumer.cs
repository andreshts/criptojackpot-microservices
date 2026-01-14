using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderCreatedEvent to confirm reservation of lottery numbers.
/// </summary>
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<OrderCreatedConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderCreatedEvent for Order {OrderId}. Reserving {Count} numbers.",
            message.OrderId, message.LotteryNumberIds.Count);

        var success = await _lotteryNumberRepository.ReserveNumbersAsync(
            message.LotteryNumberIds, 
            message.OrderId);

        if (success)
        {
            _logger.LogInformation(
                "Successfully reserved {Count} numbers for Order {OrderId}. Expires at {ExpiresAt}",
                message.LotteryNumberIds.Count, message.OrderId, message.ExpiresAt);
        }
        else
        {
            _logger.LogWarning(
                "Failed to reserve numbers for Order {OrderId}. Some numbers may not be available.",
                message.OrderId);
        }
    }
}

