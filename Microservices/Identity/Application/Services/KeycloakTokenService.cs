using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Implementation of IKeycloakTokenService for user token operations.
/// Handles login (ROPC flow), token refresh, and token revocation.
/// Does NOT require admin authentication.
/// </summary>
public class KeycloakTokenService : IKeycloakTokenService
{
    private const string BearerScheme = "Bearer";
    
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakTokenService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KeycloakTokenService(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakTokenService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<KeycloakTokenResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret ?? "",
            ["username"] = email,
            ["password"] = password,
            ["scope"] = "openid email profile"
        });

        var response = await _httpClient.PostAsync(_settings.GetTokenUrl(), tokenRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get token for user {Email}: {StatusCode}", email, response.StatusCode);
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponseInternal>(_jsonOptions, cancellationToken);
        if (tokenResponse == null) return null;

        return MapToPublicResponse(tokenResponse);
    }

    public async Task<KeycloakTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret ?? "",
            ["refresh_token"] = refreshToken
        });

        var response = await _httpClient.PostAsync(_settings.GetTokenUrl(), tokenRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to refresh token: {StatusCode}", response.StatusCode);
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponseInternal>(_jsonOptions, cancellationToken);
        if (tokenResponse == null) return null;

        return MapToPublicResponse(tokenResponse);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var logoutUrl = _settings.GetLogoutUrl();
        
        var revokeRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret ?? "",
            ["refresh_token"] = refreshToken
        });

        var response = await _httpClient.PostAsync(logoutUrl, revokeRequest, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to revoke token: {StatusCode}", response.StatusCode);
            return false;
        }

        _logger.LogDebug("Successfully revoked refresh token");
        return true;
    }

    private static KeycloakTokenResponse MapToPublicResponse(KeycloakTokenResponseInternal tokenResponse)
    {
        return new KeycloakTokenResponse
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            RefreshExpiresIn = tokenResponse.RefreshExpiresIn,
            TokenType = tokenResponse.TokenType ?? BearerScheme,
            IdToken = tokenResponse.IdToken
        };
    }
}
