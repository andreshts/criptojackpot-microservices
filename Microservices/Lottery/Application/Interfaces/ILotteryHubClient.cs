using CryptoJackpot.Lottery.Application.DTOs;

namespace CryptoJackpot.Lottery.Application.Interfaces;

/// <summary>
/// Interface for SignalR client methods.
/// Defines the methods that can be called on connected clients.
/// </summary>
public interface ILotteryHubClient
{
    /// <summary>
    /// Notifies clients when a number has been reserved.
    /// </summary>
    Task NumberReserved(Guid lotteryId, Guid numberId, int number, int series);

    /// <summary>
    /// Notifies clients when a number has been released (available again).
    /// </summary>
    Task NumberReleased(Guid lotteryId, Guid numberId, int number, int series);

    /// <summary>
    /// Notifies clients when a number has been sold (permanently unavailable).
    /// </summary>
    Task NumberSold(Guid lotteryId, Guid numberId, int number, int series);

    /// <summary>
    /// Notifies clients when multiple numbers have been released.
    /// </summary>
    Task NumbersReleased(Guid lotteryId, List<NumberStatusDto> numbers);

    /// <summary>
    /// Notifies clients when multiple numbers have been sold.
    /// </summary>
    Task NumbersSold(Guid lotteryId, List<NumberStatusDto> numbers);

    /// <summary>
    /// Sends the current available numbers to a client.
    /// </summary>
    Task ReceiveAvailableNumbers(Guid lotteryId, List<AvailableNumberDto> numbers);

    /// <summary>
    /// Confirms a reservation to the requesting client.
    /// </summary>
    Task ReservationConfirmed(NumberReservationDto reservation);

    /// <summary>
    /// Sends an error message to a client.
    /// </summary>
    Task ReceiveError(string message);
}

