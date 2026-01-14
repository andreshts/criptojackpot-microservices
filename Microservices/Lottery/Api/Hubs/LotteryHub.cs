using CryptoJackpot.Lottery.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time lottery number updates.
/// Clients connect to this hub to receive instant notifications when numbers are reserved/released/sold.
/// </summary>
[Authorize]
public class LotteryHub : Hub<ILotteryHubClient>
{
    private readonly ILotteryNumberService _lotteryNumberService;
    private readonly ILogger<LotteryHub> _logger;

    public LotteryHub(
        ILotteryNumberService lotteryNumberService,
        ILogger<LotteryHub> logger)
    {
        _lotteryNumberService = lotteryNumberService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to LotteryHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected from LotteryHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a lottery room to receive updates for a specific lottery.
    /// </summary>
    /// <param name="lotteryId">The lottery ID to join</param>
    public async Task JoinLottery(Guid lotteryId)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined lottery {LotteryId}", Context.ConnectionId, lotteryId);
        
        // Send current available numbers to the newly connected client
        var availableNumbers = await _lotteryNumberService.GetAvailableNumbersAsync(lotteryId);
        await Clients.Caller.ReceiveAvailableNumbers(lotteryId, availableNumbers);
    }

    /// <summary>
    /// Leave a lottery room.
    /// </summary>
    /// <param name="lotteryId">The lottery ID to leave</param>
    public async Task LeaveLottery(Guid lotteryId)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left lottery {LotteryId}", Context.ConnectionId, lotteryId);
    }

    /// <summary>
    /// Reserve a number for the current user.
    /// This will broadcast the reservation to all connected clients.
    /// </summary>
    /// <param name="lotteryId">The lottery ID</param>
    /// <param name="number">The number to reserve</param>
    /// <param name="series">The series (optional, if not specified, first available series is used)</param>
    public async Task ReserveNumber(Guid lotteryId, int number, int? series = null)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            await Clients.Caller.ReceiveError("Unauthorized");
            return;
        }

        var result = await _lotteryNumberService.ReserveNumberAsync(lotteryId, number, series, userId.Value);
        
        if (result.IsSuccess)
        {
            var reservation = result.Value;
            
            // Notify all clients in the lottery group
            var groupName = GetLotteryGroupName(lotteryId);
            await Clients.Group(groupName).NumberReserved(
                lotteryId, 
                reservation.NumberId, 
                reservation.Number, 
                reservation.Series);
            
            // Confirm to the caller
            await Clients.Caller.ReservationConfirmed(reservation);
            
            _logger.LogInformation(
                "Number {Number} series {Series} reserved by user {UserId} in lottery {LotteryId}",
                number, reservation.Series, userId, lotteryId);
        }
        else
        {
            await Clients.Caller.ReceiveError(result.Errors.First().Message);
        }
    }

    /// <summary>
    /// Get the group name for a lottery.
    /// </summary>
    private static string GetLotteryGroupName(Guid lotteryId) => $"lottery-{lotteryId}";

    /// <summary>
    /// Get the current user ID from the connection context.
    /// </summary>
    private long? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

