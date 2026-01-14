using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderCompletedEvent to mark lottery numbers as sold permanently.
/// Broadcasts the sale via SignalR to all connected clients.
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryNotificationService _notificationService;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryNotificationService notificationService,
        ILogger<OrderCompletedConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _notificationService = notificationService;
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
            // Get the sold numbers for broadcast
            var numbers = await _lotteryNumberRepository.GetByIdsAsync(message.LotteryNumberIds);
            
            // Broadcast via SignalR
            var soldNumbers = numbers.Select(n => new NumberStatusDto
            {
                NumberId = n.Id,
                Number = n.Number,
                Series = n.Series,
                Status = NumberStatus.Sold
            }).ToList();

            await _notificationService.NotifyNumbersSoldAsync(message.LotteryId, soldNumbers);

            _logger.LogInformation(
                "Successfully confirmed and broadcasted {Count} numbers as sold for Ticket {TicketId}. Transaction: {TransactionId}",
                soldNumbers.Count, message.TicketId, message.TransactionId);
        }
        else
        {
            _logger.LogError(
                "Failed to confirm numbers as sold for Order {OrderId}. Numbers may not be in Reserved status.",
                message.OrderId);
        }
    }
}

