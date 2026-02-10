using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.Http;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Implementation of IKeycloakUserService for user CRUD operations.
/// Token management is handled by KeycloakAdminTokenHandler.
/// </summary>
public class KeycloakUserService : IKeycloakUserService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakUserService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KeycloakUserService(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakUserService> logger)
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

    private string AdminUrl => _settings.GetAdminUrl();

    public async Task<string?> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        bool emailVerified = false,
        CancellationToken cancellationToken = default)
    {
        return await CreateUserAsync(email, firstName, lastName, password, emailVerified, null, cancellationToken);
    }

    public async Task<string?> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        string? password = null,
        bool emailVerified = false,
        Dictionary<string, List<string>>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userRepresentation = new
            {
                email,
                username = email,
                firstName,
                lastName,
                emailVerified,
                enabled = true,
                attributes = attributes ?? new Dictionary<string, List<string>>(),
                credentials = password != null ? new[]
                {
                    new { type = "password", value = password, temporary = false }
                } : null,
                requiredActions = password == null ? new[] { "UPDATE_PASSWORD" } : Array.Empty<string>()
            };

            var url = $"{AdminUrl}{KeycloakEndpoints.Users.Base}";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(userRepresentation, options: _jsonOptions)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogWarning("User with email {Email} already exists in Keycloak", email);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create user {Email} in Keycloak: {StatusCode}", email, response.StatusCode);
                return null;
            }

            var locationHeader = response.Headers.Location?.ToString();
            var keycloakUserId = locationHeader?.Split('/').LastOrDefault();

            if (string.IsNullOrEmpty(keycloakUserId))
            {
                _logger.LogError("Failed to get Keycloak user ID from response for {Email}", email);
                return null;
            }

            _logger.LogInformation("Created user {Email} in Keycloak with ID {KeycloakUserId}", email, keycloakUserId);
            return keycloakUserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email} in Keycloak", email);
            return null;
        }
    }

    public async Task UpdateUserAsync(
        string keycloakUserId,
        string? firstName = null,
        string? lastName = null,
        bool? emailVerified = null,
        bool? enabled = null,
        Dictionary<string, List<string>>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        var updates = new Dictionary<string, object>();
        if (firstName != null) updates["firstName"] = firstName;
        if (lastName != null) updates["lastName"] = lastName;
        if (emailVerified.HasValue) updates["emailVerified"] = emailVerified.Value;
        if (enabled.HasValue) updates["enabled"] = enabled.Value;
        if (attributes != null) updates["attributes"] = attributes;

        var url = $"{AdminUrl}{KeycloakEndpoints.Users.ById(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(updates, options: _jsonOptions)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Updated user {KeycloakUserId} in Keycloak", keycloakUserId);
    }

    public async Task DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Users.ById(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Deleted user {KeycloakUserId} from Keycloak", keycloakUserId);
    }

    public async Task<KeycloakUserDto?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Users.ById(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KeycloakUserDto>(_jsonOptions, cancellationToken);
    }

    public async Task<KeycloakUserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Users.ByEmail(email)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(_jsonOptions, cancellationToken);
        return users?.FirstOrDefault();
    }

    public async Task SendVerificationEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Users.SendVerifyEmail(keycloakUserId)}"
                  + "?client_id=cryptojackpot-frontend";
    
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Sent verification email to user {KeycloakUserId}", keycloakUserId);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{AdminUrl}{KeycloakEndpoints.Users.ExecuteActionsEmail(keycloakUserId)}";
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(new[] { "UPDATE_PASSWORD" })
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send password reset email to user {KeycloakUserId}: {StatusCode}", keycloakUserId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Sent password reset email to user {KeycloakUserId}", keycloakUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to user {KeycloakUserId}", keycloakUserId);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string keycloakUserId, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{AdminUrl}{KeycloakEndpoints.Users.ResetPassword(keycloakUserId)}";
            var passwordCredential = new
            {
                type = "password",
                value = newPassword,
                temporary = false
            };

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(passwordCredential, options: _jsonOptions)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to reset password for user {KeycloakUserId}: {StatusCode}", keycloakUserId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Reset password for user {KeycloakUserId}", keycloakUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {KeycloakUserId}", keycloakUserId);
            return false;
        }
    }

    public async Task SetUserEnabledAsync(string keycloakUserId, bool enabled, CancellationToken cancellationToken = default)
    {
        await UpdateUserAsync(keycloakUserId, enabled: enabled, cancellationToken: cancellationToken);
    }

    public async Task LogoutAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Users.Logout(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Logged out user {KeycloakUserId} from all sessions", keycloakUserId);
    }
}
