using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces;

/// <summary>
/// Repository for 2FA recovery code operations.
/// </summary>
public interface IRecoveryCodeRepository
{
    /// <summary>
    /// Gets all unused recovery codes for a user.
    /// </summary>
    Task<IReadOnlyList<UserRecoveryCode>> GetUnusedByUserIdAsync(long userId);

    /// <summary>
    /// Gets count of remaining unused codes.
    /// </summary>
    Task<int> GetRemainingCountAsync(long userId);

    /// <summary>
    /// Adds multiple recovery codes (when enabling 2FA).
    /// </summary>
    Task AddRangeAsync(IEnumerable<UserRecoveryCode> codes);

    /// <summary>
    /// Deletes all recovery codes for a user (when disabling 2FA).
    /// </summary>
    Task DeleteAllByUserIdAsync(long userId);
}

