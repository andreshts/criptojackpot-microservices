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
            _logger.LogInformation("JoinLottery called with lotteryId: {LotteryId}", lotteryId);
            
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
    /// Reserve N series of a number for the current user.
    /// The system automatically assigns the next available series in order.
    /// Example: User requests number 10 with quantity 2 → System assigns Series 1 and Series 2 (if available)
    /// </summary>
    /// <param name="lotteryId">The lottery ID</param>
    /// <param name="number">The number to reserve (e.g., 10)</param>
    /// <param name="quantity">How many series to reserve (default: 1)</param>
    [Obsolete("Use ReserveNumberWithOrder instead. This method will be removed in future versions.")]
    public async Task ReserveNumber(Guid lotteryId, int number, int quantity = 1)
    {
        try
        {
            _logger.LogInformation(
                "ReserveNumber called - LotteryId: {LotteryId}, Number: {Number}, Quantity: {Quantity}",
                lotteryId, number, quantity);

            var userId = GetUserId();
            _logger.LogInformation("UserId from token: {UserId}", userId);
            
            if (userId == null)
            {
                _logger.LogWarning("ReserveNumber failed: Unauthorized - no userId in token");
                await Clients.Caller.ReceiveError("Unauthorized");
                return;
            }

            _logger.LogInformation("Calling ReserveNumberByQuantityAsync...");
            var result = await _lotteryNumberService.ReserveNumberByQuantityAsync(lotteryId, number, quantity, userId.Value);
            _logger.LogInformation("ReserveNumberByQuantityAsync result - IsSuccess: {IsSuccess}", result.IsSuccess);
            
            if (result.IsSuccess)
            {
                var reservations = result.Value;
                _logger.LogInformation("Reservations count: {Count}", reservations.Count);
                
                var groupName = GetLotteryGroupName(lotteryId);
                
                // Notify all clients in the lottery group about each reserved series
                foreach (var reservation in reservations)
                {
                    _logger.LogInformation(
                        "Notifying group about reservation - Number: {Number}, Series: {Series}",
                        reservation.Number, reservation.Series);
                        
                    await Clients.Group(groupName).NumberReserved(
                        lotteryId, 
                        reservation.NumberId, 
                        reservation.Number, 
                        reservation.Series);
                }
                
                // Confirm all reservations to the caller
                await Clients.Caller.ReservationsConfirmed(reservations);
                
                _logger.LogInformation(
                    "User {UserId} reserved {Count} series of number {Number} in lottery {LotteryId}. Series: [{Series}]",
                    userId, 
                    reservations.Count, 
                    number, 
                    lotteryId,
                    string.Join(", ", reservations.Select(r => r.Series)));
            }
            else
            {
                var errorMessage = result.Errors.First().Message;
                _logger.LogWarning("ReserveNumber failed: {Error}", errorMessage);
                await Clients.Caller.ReceiveError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ReserveNumber - LotteryId: {LotteryId}, Number: {Number}, Quantity: {Quantity}",
                lotteryId, number, quantity);
            await Clients.Caller.ReceiveError($"Error reserving number: {ex.Message}");
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
                var errorMessage = result.Errors.First().Message;
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

