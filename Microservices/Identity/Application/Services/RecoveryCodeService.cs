using System.Security.Cryptography;
using System.Text;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Recovery code service for 2FA backup codes.
/// Generates codes in format XXXX-XXXX and stores SHA-256 hashes.
/// </summary>
public class RecoveryCodeService : IRecoveryCodeService
{
    private const int CodePartLength = 4;
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude confusing chars: 0,O,1,I

    public (IReadOnlyList<string> PlainCodes, IReadOnlyList<UserRecoveryCode> Entities) GenerateCodes(
        long userId, 
        int count = 8)
    {
        var plainCodes = new List<string>(count);
        var entities = new List<UserRecoveryCode>(count);

        for (var i = 0; i < count; i++)
        {
            var plainCode = GenerateSingleCode();
            var codeHash = HashCode(plainCode);

            plainCodes.Add(plainCode);
            entities.Add(new UserRecoveryCode
            {
                UserId = userId,
                CodeHash = codeHash,
                IsUsed = false
            });
        }

        return (plainCodes.AsReadOnly(), entities.AsReadOnly());
    }

    public UserRecoveryCode? ValidateCode(string plainCode, IEnumerable<UserRecoveryCode> storedCodes)
    {
        if (string.IsNullOrWhiteSpace(plainCode))
            return null;

        // Normalize: remove dashes, convert to uppercase
        var normalizedCode = plainCode.Replace("-", "").ToUpperInvariant();
        var codeHash = HashCode(normalizedCode);

        return storedCodes.FirstOrDefault(c => !c.IsUsed && c.CodeHash == codeHash);
    }

    public string HashCode(string plainCode)
    {
        // Normalize before hashing
        var normalizedCode = plainCode.Replace("-", "").ToUpperInvariant();
        var codeBytes = Encoding.UTF8.GetBytes(normalizedCode);
        var hashBytes = SHA256.HashData(codeBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string GenerateSingleCode()
    {
        var part1 = GenerateRandomPart();
        var part2 = GenerateRandomPart();
        return $"{part1}-{part2}";
    }

    private static string GenerateRandomPart()
    {
        var chars = new char[CodePartLength];
        var randomBytes = RandomNumberGenerator.GetBytes(CodePartLength);

        for (var i = 0; i < CodePartLength; i++)
        {
            chars[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
        }

        return new string(chars);
    }
}

