namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Request message for getting all users from Identity service.
/// Used by Notification service to get user list for marketing emails.
/// </summary>
public record GetAllUsersRequest
{
    /// <summary>
    /// If true, only returns users with confirmed emails
    /// </summary>
    public bool OnlyConfirmedEmails { get; init; } = true;
    
    /// <summary>
    /// If true, only returns active users
    /// </summary>
    public bool OnlyActiveUsers { get; init; } = true;
}
