using System.Security.Cryptography;
using System.Text;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Refresh token service with SHA-256 hashing and rotation support.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private const int TokenLengthBytes = 64; // 512 bits
    private const int DefaultExpirationDays = 7;
    private const int RememberMeExpirationDays = 30;

    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(TokenLengthBytes);
        var base64 = Convert.ToBase64String(randomBytes);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public (string RawToken, UserRefreshToken Entity) CreateRefreshToken(
        long userId,
        Guid? familyId = null,
        string? deviceInfo = null,
        string? ipAddress = null,
        bool rememberMe = false)
    {
        var rawToken = GenerateToken();
        var tokenHash = HashToken(rawToken);

        var entity = new UserRefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            FamilyId = familyId ?? Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(rememberMe ? RememberMeExpirationDays : DefaultExpirationDays),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            IsRevoked = false
        };

        return (rawToken, entity);
    }

    public async Task<UserRefreshToken?> ValidateAndGetTokenAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return null;

        var tokenHash = HashToken(rawToken);
        var token = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (token == null || !token.IsActive)
            return null;

        return token;
    }
}

