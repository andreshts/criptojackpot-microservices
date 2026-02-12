using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Google OAuth service for ID token validation.
/// Requires NuGet package: Google.Apis.Auth
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthConfig _config;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IOptions<GoogleAuthConfig> config,
        ILogger<GoogleAuthService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<GoogleUserPayload?> ValidateIdTokenAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // Verify issuer
            if (payload.Issuer != "accounts.google.com" && payload.Issuer != "https://accounts.google.com")
            {
                _logger.LogWarning("Invalid issuer in Google ID token: {Issuer}", payload.Issuer);
                return null;
            }

            return new GoogleUserPayload
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                EmailVerified = payload.EmailVerified,
                GivenName = payload.GivenName,
                FamilyName = payload.FamilyName,
                Name = payload.Name,
                Picture = payload.Picture
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google ID token");
            return null;
        }
    }
}

