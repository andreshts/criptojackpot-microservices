using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumer that handles LotteryCreatedEvent to send marketing emails to all users.
/// Uses MassTransit Request/Response pattern to get user list from Identity service.
/// </summary>
public class LotteryMarketingConsumer : IConsumer<LotteryCreatedEvent>
{
    private readonly ILogger<LotteryMarketingConsumer> _logger;
    private readonly IMediator _mediator;
    private readonly IRequestClient<GetAllUsersRequest> _usersClient;

    public LotteryMarketingConsumer(
        IMediator mediator, 
        IRequestClient<GetAllUsersRequest> usersClient,
        ILogger<LotteryMarketingConsumer> logger)
    {
        _logger = logger;
        _mediator = mediator;
        _usersClient = usersClient;
    }

    public async Task Consume(ConsumeContext<LotteryCreatedEvent> context)
    {
        var lottery = context.Message;
        
        _logger.LogInformation(
            "Received LotteryCreatedEvent for lottery {LotteryId} - {Title}. Starting marketing email campaign.",
            lottery.LotteryId, lottery.Title);

        try
        {
            // Request all active users from Identity service via MassTransit Request/Response
            var response = await _usersClient.GetResponse<GetAllUsersResponse>(new GetAllUsersRequest
            {
                OnlyActiveUsers = true,
                OnlyConfirmedEmails = true
            });

            if (!response.Message.Success)
            {
                _logger.LogError("Failed to get users from Identity service: {Error}", response.Message.ErrorMessage);
                return;
            }

            var users = response.Message.Users.ToList();
            _logger.LogInformation("Retrieved {Count} users for lottery marketing campaign", users.Count);

            if (users.Count == 0)
            {
                _logger.LogWarning("No users found for marketing campaign. Skipping email send.");
                return;
            }

            // Send marketing email to each user
            var successCount = 0;
            var failCount = 0;

            foreach (var user in users)
            {
                try
                {
                    await _mediator.Send(new SendLotteryMarketingEmailCommand
                    {
                        Email = user.Email,
                        UserName = user.Name,
                        UserLastName = user.LastName,
                        LotteryId = lottery.LotteryId,
                        LotteryTitle = lottery.Title,
                        LotteryDescription = lottery.Description,
                        TicketPrice = lottery.TicketPrice,
                        StartDate = lottery.StartDate,
                        EndDate = lottery.EndDate,
                        MaxTickets = lottery.MaxTickets
                    });
                    
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(ex, "Failed to send marketing email to {Email}", user.Email);
                }
            }

            _logger.LogInformation(
                "Lottery marketing campaign completed for {LotteryId}. Sent: {Success}, Failed: {Failed}",
                lottery.LotteryId, successCount, failCount);
        }
        catch (RequestTimeoutException ex)
        {
            _logger.LogError(ex, "Timeout waiting for Identity service response for lottery {LotteryId}", lottery.LotteryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing LotteryCreatedEvent for lottery {LotteryId}", lottery.LotteryId);
            throw;
        }
    }
}