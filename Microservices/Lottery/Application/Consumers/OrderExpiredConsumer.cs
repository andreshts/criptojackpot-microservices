using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderExpiredEvent to release reserved lottery numbers back to available.
/// </summary>
public class OrderExpiredConsumer : IConsumer<OrderExpiredEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<OrderExpiredConsumer> _logger;

    public OrderExpiredConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<OrderExpiredConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderExpiredEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderExpiredEvent for Order {OrderId}. Releasing {Count} reserved numbers.",
            message.OrderId, message.LotteryNumberIds.Count);

        var success = await _lotteryNumberRepository.ReleaseNumbersByOrderAsync(message.OrderId);

        if (success)
        {
            _logger.LogInformation(
                "Successfully released reserved numbers for expired Order {OrderId}",
                message.OrderId);
        }
        else
        {
            _logger.LogWarning(
                "No reserved numbers found to release for Order {OrderId}",
                message.OrderId);
        }
    }
}

