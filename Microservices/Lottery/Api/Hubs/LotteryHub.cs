using CryptoJackpot.Lottery.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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
    public async override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to LotteryHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public async override Task OnDisconnectedAsync(Exception? exception)
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
        try
        { 
            var groupName = GetLotteryGroupName(lotteryId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} joined lottery {LotteryId}", Context.ConnectionId, lotteryId);
            
            // Send current available numbers to the newly connected client
            var availableNumbers = await _lotteryNumberService.GetAvailableNumbersAsync(lotteryId);
            _logger.LogInformation("Retrieved {Count} available numbers for lottery {LotteryId}", availableNumbers.Count, lotteryId);
            
            await Clients.Caller.ReceiveAvailableNumbers(lotteryId, availableNumbers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JoinLottery for lottery {LotteryId}: {Message}", lotteryId, ex.Message);
            await Clients.Caller.ReceiveError($"Error joining lottery: {ex.Message}");
        }
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
    /// Refresh available numbers for a lottery.
    /// Useful when client suspects data is out of sync or for manual refresh.
    /// </summary>
    /// <param name="lotteryId">The lottery ID to refresh</param>
    public async Task RefreshNumbers(Guid lotteryId)
    {
        try
        {
            _logger.LogInformation("RefreshNumbers called for lottery {LotteryId}", lotteryId);
            var availableNumbers = await _lotteryNumberService.GetAvailableNumbersAsync(lotteryId);
            await Clients.Caller.ReceiveAvailableNumbers(lotteryId, availableNumbers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RefreshNumbers for lottery {LotteryId}", lotteryId);
            await Clients.Caller.ReceiveError($"Error refreshing numbers: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Reserve N series of a number and automatically create/update an order.
    /// This is the recommended method - it combines number reservation with order creation.
    /// </summary>
    /// <param name="lotteryId">The lottery ID</param>
    /// <param name="number">The number to reserve (e.g., 10)</param>
    /// <param name="quantity">How many series to reserve (default: 1)</param>
    /// <param name="existingOrderId">Optional: existing pending order to add numbers to (cart functionality)</param>
    public async Task ReserveNumberWithOrder(Guid lotteryId, int number, int quantity = 1, Guid? existingOrderId = null)
    {
        try
        {
            _logger.LogInformation(
                "ReserveNumberWithOrder called - LotteryId: {LotteryId}, Number: {Number}, Quantity: {Quantity}, ExistingOrderId: {ExistingOrderId}",
                lotteryId, number, quantity, existingOrderId);

            var userId = GetUserId();
            
            if (userId == null)
            {
                _logger.LogWarning("ReserveNumberWithOrder failed: Unauthorized - no userId in token");
                await Clients.Caller.ReceiveError("Unauthorized");
                return;
            }

            var result = await _lotteryNumberService.ReserveNumberWithOrderAsync(
                lotteryId, number, quantity, userId.Value, existingOrderId);
            
            if (result.IsSuccess)
            {
                var reservationWithOrder = result.Value;
                var groupName = GetLotteryGroupName(lotteryId);
                
                // Notify all clients in the lottery group about each reserved series
                foreach (var reservation in reservationWithOrder.Reservations)
                {
                    await Clients.Group(groupName).NumberReserved(
                        lotteryId, 
                        reservation.NumberId, 
                        reservation.Number, 
                        reservation.Series);
                }
                
                // Send reservation with order info to the caller
                await Clients.Caller.ReservationWithOrderConfirmed(reservationWithOrder);
                
                _logger.LogInformation(
                    "User {UserId} reserved {Count} numbers in lottery {LotteryId}. OrderId: {OrderId}, Amount: {Amount}",
                    userId, 
                    reservationWithOrder.Reservations.Count, 
                    lotteryId,
                    reservationWithOrder.OrderId,
                    reservationWithOrder.TotalAmount);
            }
            else
            {
                var errorMessage = result.Errors.FirstOrDefault()?.Message ?? "Unknown error";
    
                _logger.LogWarning("ReserveNumberWithOrder failed: {Error}", errorMessage);
                await Clients.Caller.ReceiveError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Exception in ReserveNumberWithOrder - LotteryId: {LotteryId}, Number: {Number}, Quantity: {Quantity}",
                lotteryId, number, quantity);
            await Clients.Caller.ReceiveError($"Error reserving number: {ex.Message}");
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
        // Try NameIdentifier first (standard .NET claim)
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Fallback to 'sub' claim (standard JWT claim)
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = Context.User?.FindFirst("sub")?.Value;
        }
        
        _logger.LogDebug("GetUserId - Claims: {Claims}", 
            string.Join(", ", Context.User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
        
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

