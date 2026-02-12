namespace CryptoJackpot.Domain.Core.Responses.Errors;

/// <summary>
/// Error for account lockout HTTP 423 Locked.
/// Includes retry-after information for the client.
/// </summary>
public class LockedError : ApplicationError
{
    /// <summary>
    /// Seconds until the lockout expires.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public LockedError(string message, int retryAfterSeconds) 
        : base(message, 423)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

