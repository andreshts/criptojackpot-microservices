using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Lottery.Application.Interfaces;

/// <summary>
/// Service interface for lottery number operations.
/// Used by SignalR Hub for real-time number management.
/// </summary>
public interface ILotteryNumberService
{
    /// <summary>
    /// Gets available numbers for a lottery (grouped by number with series count).
    /// </summary>
    Task<List<AvailableNumberDto>> GetAvailableNumbersAsync(Guid lotteryId);
    
    /// <summary>
    /// Reserves N series of a specific number for a user.
    /// The system automatically assigns the next available series in order.
    /// </summary>
    /// <param name="lotteryId">The lottery ID</param>
    /// <param name="number">The number to reserve (e.g., 10)</param>
    /// <param name="quantity">How many series to reserve (e.g., 2 means Serie 1 and Serie 2 if available)</param>
    /// <param name="userId">The user making the reservation</param>
    /// <returns>List of reservations with assigned series</returns>
    Task<Result<List<NumberReservationDto>>> ReserveNumberByQuantityAsync(Guid lotteryId, int number, int quantity, long userId);

    /// <summary>
    /// Reserves numbers from cart items and creates/updates an order.
    /// This is the primary method for the SignalR Hub - combines reservation + order creation.
    /// Supports both single number and multiple numbers (cart checkout).
    /// </summary>
    /// <param name="lotteryId">The lottery ID</param>
    /// <param name="items">Cart items to reserve (each with number and quantity)</param>
    /// <param name="userId">The user making the reservation</param>
    /// <param name="existingOrderId">Optional: existing pending order to add to</param>
    /// <returns>Reservation details with order information</returns>
    Task<Result<ReservationWithOrderDto>> ReserveNumbersWithOrderAsync(
        Guid lotteryId, 
        List<CartItemDto> items, 
        long userId,
        Guid? existingOrderId = null);

    /// <summary>
    /// Gets detailed number status for a lottery.
    /// </summary>
    Task<List<NumberStatusDto>> GetNumberStatusesAsync(Guid lotteryId);
}

