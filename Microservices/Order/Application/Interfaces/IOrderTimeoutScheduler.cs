namespace CryptoJackpot.Order.Application.Interfaces;

/// <summary>
/// Service for scheduling order timeout jobs using Quartz.NET
/// </summary>
public interface IOrderTimeoutScheduler
{
    /// <summary>
    /// Schedules a job to process order timeout at the specified time
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="lotteryId">The lottery ID associated with the order</param>
    /// <param name="lotteryNumberIds">List of lottery number IDs to release on timeout</param>
    /// <param name="expiresAt">When the order should expire (UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ScheduleOrderTimeoutAsync(
        Guid orderId,
        Guid lotteryId,
        List<Guid> lotteryNumberIds,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a scheduled order timeout job (e.g., when order is completed or cancelled)
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the job was found and cancelled, false otherwise</returns>
    Task<bool> CancelOrderTimeoutAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reschedules an existing order timeout to a new time
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="newExpiresAt">The new expiration time (UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the job was found and rescheduled, false otherwise</returns>
    Task<bool> RescheduleOrderTimeoutAsync(
        Guid orderId, 
        DateTime newExpiresAt,
        CancellationToken cancellationToken = default);
}
