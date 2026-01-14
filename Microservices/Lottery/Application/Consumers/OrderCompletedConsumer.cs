using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderCompletedEvent to mark lottery numbers as sold permanently.
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<OrderCompletedConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderCompletedEvent for Order {OrderId}. Confirming {Count} numbers as sold.",
            message.OrderId, message.LotteryNumberIds.Count);

        var success = await _lotteryNumberRepository.ConfirmNumbersSoldAsync(
            message.LotteryNumberIds, 
            message.TicketId);

        if (success)
        {
            _logger.LogInformation(
                "Successfully confirmed {Count} numbers as sold for Ticket {TicketId}. Transaction: {TransactionId}",
                message.LotteryNumberIds.Count, message.TicketId, message.TransactionId);
        }
        else
        {
            _logger.LogError(
                "Failed to confirm numbers as sold for Order {OrderId}. Numbers may not be in Reserved status.",
                message.OrderId);
        }
    }
}

