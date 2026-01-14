using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderCancelledEvent to release reserved lottery numbers back to available.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<OrderCancelledConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderCancelledEvent for Order {OrderId}. Reason: {Reason}. Releasing {Count} reserved numbers.",
            message.OrderId, message.Reason, message.LotteryNumberIds.Count);

        var success = await _lotteryNumberRepository.ReleaseNumbersByOrderAsync(message.OrderId);

        if (success)
        {
            _logger.LogInformation(
                "Successfully released reserved numbers for cancelled Order {OrderId}",
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

