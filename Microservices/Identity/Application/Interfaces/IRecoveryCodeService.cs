using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for 2FA recovery code operations.
/// </summary>
public interface IRecoveryCodeService
{
    /// <summary>
    /// Generates a set of recovery codes for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="count">Number of codes to generate (default: 8)</param>
    /// <returns>Tuple of (plain text codes to show user once, entities for storage)</returns>
    (IReadOnlyList<string> PlainCodes, IReadOnlyList<UserRecoveryCode> Entities) GenerateCodes(
        long userId, 
        int count = 8);

    /// <summary>
    /// Validates a recovery code against stored hashes.
    /// </summary>
    /// <param name="plainCode">Code entered by user (format: XXXX-XXXX)</param>
    /// <param name="storedCodes">User's stored recovery codes</param>
    /// <returns>The matching code entity if valid, null otherwise</returns>
    UserRecoveryCode? ValidateCode(string plainCode, IEnumerable<UserRecoveryCode> storedCodes);

    /// <summary>
    /// Computes SHA-256 hash of a recovery code for storage.
    /// </summary>
    string HashCode(string plainCode);
}

