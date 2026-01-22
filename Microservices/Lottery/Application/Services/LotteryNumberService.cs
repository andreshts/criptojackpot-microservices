using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Services;

/// <summary>
/// Service for lottery number operations.
/// Handles real-time number reservations for SignalR hub.
/// </summary>
public class LotteryNumberService : ILotteryNumberService
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<LotteryNumberService> _logger;
    private const int ReservationMinutes = 5;

    public LotteryNumberService(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        IEventBus eventBus,
        ILogger<LotteryNumberService> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<List<AvailableNumberDto>> GetAvailableNumbersAsync(Guid lotteryGuid)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(lotteryGuid);
        if (lottery == null)
            return [];

        var numbers = await _lotteryNumberRepository.GetNumbersByLotteryAsync(lottery.Id);

        // Group by number and count available series
        var grouped = numbers
            .GroupBy(n => n.Number)
            .Select(g => new AvailableNumberDto
            {
                Number = g.Key,
                AvailableSeries = g.Count(n => n.Status == NumberStatus.Available),
                TotalSeries = g.Count()
            })
            .OrderBy(n => n.Number)
            .ToList();

        return grouped;
    }

    public async Task<Result<NumberReservationDto>> ReserveNumberAsync(
        Guid lotteryGuid,
        int number,
        int? series,
        long userId)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(lotteryGuid);
        if (lottery == null)
            return Result.Fail<NumberReservationDto>("Lottery not found");

        // Find available number (first available series if not specified)
        var availableNumber = await _lotteryNumberRepository.FindAvailableNumberAsync(
            lottery.Id, number, series);

        if (availableNumber == null)
        {
            var message = series.HasValue
                ? $"Number {number} series {series} is not available"
                : $"Number {number} is not available in any series";

            _logger.LogWarning(
                "Failed to reserve number {Number} series {Series} in lottery {LotteryId}: not available",
                number, series, lotteryGuid);

            return Result.Fail<NumberReservationDto>(message);
        }

        // Reserve the number
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(ReservationMinutes);

        availableNumber.Status = NumberStatus.Reserved;
        availableNumber.ReservationExpiresAt = expiresAt;
        availableNumber.UpdatedAt = now;

        await _lotteryNumberRepository.UpdateAsync(availableNumber);

        _logger.LogInformation(
            "Number {Number} series {Series} reserved for user {UserId} in lottery {LotteryId}. Expires at {ExpiresAt}",
            availableNumber.Number, availableNumber.Series, userId, lotteryGuid, expiresAt);

        return Result.Ok(new NumberReservationDto
        {
            NumberId = availableNumber.Id,
            LotteryNumberGuid = availableNumber.LotteryNumberGuid,
            LotteryGuid = lotteryGuid,
            Number = availableNumber.Number,
            Series = availableNumber.Series,
            ReservationExpiresAt = expiresAt,
            SecondsRemaining = ReservationMinutes * 60
        });
    }

    public async Task<List<NumberStatusDto>> GetNumberStatusesAsync(Guid lotteryGuid)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(lotteryGuid);
        if (lottery == null)
            return [];

        var numbers = await _lotteryNumberRepository.GetNumbersByLotteryAsync(lottery.Id);

        return numbers.Select(n => new NumberStatusDto
        {
            NumberId = n.Id,
            LotteryNumberGuid = n.LotteryNumberGuid,
            Number = n.Number,
            Series = n.Series,
            Status = n.Status
        }).ToList();
    }

    /// <summary>
    /// Maximum number of series a user can reserve in a single request.
    /// Prevents abuse where a malicious user could block all numbers.
    /// </summary>
    private const int MaxQuantityPerRequest = 10;

    public async Task<Result<List<NumberReservationDto>>> ReserveNumberByQuantityAsync(
        Guid lotteryGuid,
        int number,
        int quantity,
        long userId)
    {
        // Validation: quantity must be positive
        if (quantity <= 0)
            return Result.Fail<List<NumberReservationDto>>("Quantity must be greater than 0");

        // Validation: prevent abuse by limiting max quantity per request
        if (quantity > MaxQuantityPerRequest)
        {
            _logger.LogWarning(
                "User {UserId} attempted to reserve {Quantity} series (max allowed: {Max}) for number {Number} in lottery {LotteryId}",
                userId, quantity, MaxQuantityPerRequest, number, lotteryGuid);

            return Result.Fail<List<NumberReservationDto>>(
                $"Maximum {MaxQuantityPerRequest} series can be reserved per request");
        }

        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(lotteryGuid);
        if (lottery == null)
            return Result.Fail<List<NumberReservationDto>>("Lottery not found");

        // Get the next N available series for this number, ordered by series ASC
        // Uses pessimistic locking (FOR UPDATE SKIP LOCKED) to prevent race conditions
        var availableNumbers = await _lotteryNumberRepository.GetNextAvailableSeriesAsync(
            lottery.Id, number, quantity);

        if (availableNumbers.Count == 0)
        {
            _logger.LogWarning(
                "Failed to reserve number {Number} in lottery {LotteryId}: no series available",
                number, lotteryGuid);

            return Result.Fail<List<NumberReservationDto>>($"Number {number} is not available in any series");
        }

        if (availableNumbers.Count < quantity)
        {
            _logger.LogWarning(
                "Insufficient stock for number {Number} in lottery {LotteryId}. Requested: {Requested}, Available: {Available}",
                number, lotteryGuid, quantity, availableNumbers.Count);

            return Result.Fail<List<NumberReservationDto>>(
                $"Insufficient stock. Requested {quantity} series of number {number}, but only {availableNumbers.Count} available");
        }

        // Reserve all the numbers
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(ReservationMinutes);
        var reservations = new List<NumberReservationDto>();

        foreach (var availableNumber in availableNumbers)
        {
            availableNumber.Status = NumberStatus.Reserved;
            availableNumber.ReservationExpiresAt = expiresAt;
            availableNumber.UpdatedAt = now;

            reservations.Add(new NumberReservationDto
            {
                NumberId = availableNumber.Id,
                LotteryNumberGuid = availableNumber.LotteryNumberGuid,
                LotteryGuid = lotteryGuid,
                Number = availableNumber.Number,
                Series = availableNumber.Series,
                ReservationExpiresAt = expiresAt,
                SecondsRemaining = ReservationMinutes * 60
            });
        }

        await _lotteryNumberRepository.UpdateRangeAsync(availableNumbers);

        _logger.LogInformation(
            "Reserved {Count} series of number {Number} for user {UserId} in lottery {LotteryId}. Series: [{Series}]. Expires at {ExpiresAt}",
            availableNumbers.Count,
            number,
            userId,
            lotteryGuid,
            string.Join(", ", availableNumbers.Select(n => n.Series)),
            expiresAt);

        return Result.Ok(reservations);
    }

    /// <summary>
    /// Reserve multiple series of a number with an associated OrderId.
    /// This ensures the OrderId is set on the LotteryNumber records so the 
    /// MassTransit scheduler can properly release them when they expire.
    /// </summary>
    private async Task<Result<List<NumberReservationDto>>> ReserveNumberByQuantityWithOrderAsync(
        Guid lotteryGuid,
        int number,
        int quantity,
        long userId,
        Guid orderId)
    {
        // Validation: quantity must be positive
        if (quantity <= 0)
            return Result.Fail<List<NumberReservationDto>>("Quantity must be greater than 0");

        // Validation: prevent abuse by limiting max quantity per request
        if (quantity > MaxQuantityPerRequest)
        {
            _logger.LogWarning(
                "User {UserId} attempted to reserve {Quantity} series (max allowed: {Max}) for number {Number} in lottery {LotteryId}",
                userId, quantity, MaxQuantityPerRequest, number, lotteryGuid);

            return Result.Fail<List<NumberReservationDto>>(
                $"Maximum {MaxQuantityPerRequest} series can be reserved per request");
        }

        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(lotteryGuid);
        if (lottery == null)
            return Result.Fail<List<NumberReservationDto>>("Lottery not found");

        // Get the next N available series for this number, ordered by series ASC
        // Uses pessimistic locking (FOR UPDATE SKIP LOCKED) to prevent race conditions
        var availableNumbers = await _lotteryNumberRepository.GetNextAvailableSeriesAsync(
            lottery.Id, number, quantity);

        if (availableNumbers.Count == 0)
        {
            _logger.LogWarning(
                "Failed to reserve number {Number} in lottery {LotteryId}: no series available",
                number, lotteryGuid);

            return Result.Fail<List<NumberReservationDto>>($"Number {number} is not available in any series");
        }

        if (availableNumbers.Count < quantity)
        {
            _logger.LogWarning(
                "Insufficient stock for number {Number} in lottery {LotteryId}. Requested: {Requested}, Available: {Available}",
                number, lotteryGuid, quantity, availableNumbers.Count);

            return Result.Fail<List<NumberReservationDto>>(
                $"Insufficient stock. Requested {quantity} series of number {number}, but only {availableNumbers.Count} available");
        }

        // Reserve all the numbers WITH OrderId
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(ReservationMinutes);
        var reservations = new List<NumberReservationDto>();

        foreach (var availableNumber in availableNumbers)
        {
            availableNumber.Status = NumberStatus.Reserved;
            availableNumber.ReservationExpiresAt = expiresAt;
            availableNumber.OrderId = orderId; // <-- KEY FIX: Assign OrderId
            availableNumber.UpdatedAt = now;

            reservations.Add(new NumberReservationDto
            {
                NumberId = availableNumber.Id,
                LotteryNumberGuid = availableNumber.LotteryNumberGuid,
                LotteryGuid = lotteryGuid,
                Number = availableNumber.Number,
                Series = availableNumber.Series,
                ReservationExpiresAt = expiresAt,
                SecondsRemaining = ReservationMinutes * 60
            });
        }

        await _lotteryNumberRepository.UpdateRangeAsync(availableNumbers);

        _logger.LogInformation(
            "Reserved {Count} series of number {Number} for user {UserId} in lottery {LotteryId}. OrderId: {OrderId}, Series: [{Series}]. Expires at {ExpiresAt}",
            availableNumbers.Count,
            number,
            userId,
            lotteryGuid,
            orderId,
            string.Join(", ", availableNumbers.Select(n => n.Series)),
            expiresAt);

        return Result.Ok(reservations);
    }

    public async Task<Result<ReservationWithOrderDto>> ReserveNumbersWithOrderAsync(
        Guid lotteryGuid,
        List<CartItemDto> items,
        long userId,
        Guid? existingOrderId = null)
    {
        // Validation
        if (items.Count == 0)
        {
            return Result.Fail<ReservationWithOrderDto>("At least one item is required");
        }

        // First, get the lottery to get the ticket price
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(lotteryGuid);
        if (lottery == null)
        {
            return Result.Fail<ReservationWithOrderDto>("Lottery not found");
        }

        // Generate order ID FIRST (or use existing one)
        var orderId = existingOrderId ?? Guid.NewGuid();
        var isAddToExisting = existingOrderId.HasValue;

        // Reserve all numbers from the cart
        var allReservations = new List<NumberReservationDto>();

        foreach (var item in items)
        {
            var reservationResult = await ReserveNumberByQuantityWithOrderAsync(
                lotteryGuid, item.Number, item.Quantity, userId, orderId);
            if (reservationResult.IsFailed)
            {
                // If any reservation fails, we should release the already reserved numbers
                // For now, log the error and continue (the timeout will release them)
                _logger.LogWarning(
                    "Failed to reserve number {Number} x{Quantity} for user {UserId}: {Error}",
                    item.Number, item.Quantity, userId, reservationResult.Errors.FirstOrDefault()?.Message);

                return Result.Fail<ReservationWithOrderDto>(reservationResult.Errors);
            }

            allReservations.AddRange(reservationResult.Value);
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(ReservationMinutes);

        // Calculate total amount for all reservations
        var ticketPrice = lottery.TicketPrice;
        var reservationAmount = ticketPrice * allReservations.Count;

        // Publish event to Order microservice to create/update the order
        await _eventBus.Publish(new NumbersReservedEvent
        {
            OrderId = orderId,
            LotteryId = lotteryGuid,
            UserId = userId,
            LotteryNumberIds = allReservations.Select(r => r.LotteryNumberGuid).ToList(),
            Numbers = allReservations.Select(r => r.Number).ToArray(),
            SeriesArray = allReservations.Select(r => r.Series).ToArray(),
            TicketPrice = ticketPrice,
            TotalAmount = reservationAmount,
            ExpiresAt = expiresAt,
            IsAddToExistingOrder = isAddToExisting,
            ExistingOrderId = existingOrderId
        });

        _logger.LogInformation(
            "Published NumbersReservedEvent for user {UserId}. OrderId: {OrderId}, LotteryId: {LotteryId}, Items: {ItemCount}, Count: {Count}, Amount: {Amount}",
            userId, orderId, lotteryGuid, items.Count, allReservations.Count, reservationAmount);

        return Result.Ok(new ReservationWithOrderDto
        {
            OrderId = orderId,
            LotteryGuid = lotteryGuid,
            TotalAmount = reservationAmount,
            TicketPrice = ticketPrice,
            ExpiresAt = expiresAt,
            SecondsRemaining = ReservationMinutes * 60,
            Reservations = allReservations,
            AddedToExistingOrder = isAddToExisting
        });
    }
}