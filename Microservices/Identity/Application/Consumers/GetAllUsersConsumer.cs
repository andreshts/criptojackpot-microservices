using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Consumers;

/// <summary>
/// Consumer that handles GetAllUsersRequest from other microservices.
/// Returns a list of users for cross-service communication (e.g., marketing emails).
/// </summary>
public class GetAllUsersConsumer : IConsumer<GetAllUsersRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetAllUsersConsumer> _logger;

    public GetAllUsersConsumer(
        IUserRepository userRepository,
        ILogger<GetAllUsersConsumer> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetAllUsersRequest> context)
    {
        _logger.LogInformation("Received GetAllUsersRequest. OnlyConfirmedEmails: {OnlyConfirmed}, OnlyActiveUsers: {OnlyActive}",
            context.Message.OnlyConfirmedEmails, context.Message.OnlyActiveUsers);

        try
        {
            var users = await _userRepository.GetAllAsync();
            
            // Filter based on request options
            var filteredUsers = users.Where(u => 
                (!context.Message.OnlyActiveUsers || u.Status));

            var userInfoList = filteredUsers.Select(u => new UserInfo
            {
                UserGuid = u.UserGuid,
                Email = u.Email,
                Name = u.Name,
                LastName = u.LastName
            }).ToList();

            _logger.LogInformation("Returning {Count} users in response to GetAllUsersRequest", userInfoList.Count);

            await context.RespondAsync(new GetAllUsersResponse
            {
                Users = userInfoList,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllUsersRequest");
            
            await context.RespondAsync(new GetAllUsersResponse
            {
                Users = [],
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }
}
