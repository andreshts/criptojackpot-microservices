using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces;

/// <summary>
/// Repository for refresh token operations with rotation support.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its hash.
    /// </summary>
    Task<UserRefreshToken?> GetByHashAsync(string tokenHash);

    /// <summary>
    /// Gets all active tokens for a user (for session management UI).
    /// </summary>
    Task<IReadOnlyList<UserRefreshToken>> GetActiveByUserIdAsync(long userId);

    /// <summary>
    /// Gets all tokens in a family (for rotation/revocation).
    /// </summary>
    Task<IReadOnlyList<UserRefreshToken>> GetByFamilyIdAsync(Guid familyId);

    /// <summary>
    /// Adds a new refresh token.
    /// </summary>
    Task AddAsync(UserRefreshToken token);

    /// <summary>
    /// Revokes all tokens for a user (logout from all devices).
    /// </summary>
    Task RevokeAllByUserIdAsync(long userId, string reason);

    /// <summary>
    /// Revokes all tokens in a family (token reuse detected).
    /// </summary>
    Task RevokeByFamilyIdAsync(Guid familyId, string reason);

    /// <summary>
    /// Deletes expired and revoked tokens older than specified days (cleanup job).
    /// </summary>
    Task<int> DeleteExpiredAsync(int olderThanDays = 90);
}

