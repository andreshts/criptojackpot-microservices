using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Implementation of IKeycloakAdminService that communicates with Keycloak Admin REST API.
/// </summary>
public class KeycloakAdminService : IKeycloakAdminService
{
    private const string BearerScheme = "Bearer";
    
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakAdminService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private string? _adminToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public KeycloakAdminService(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakAdminService> logger)
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
    private string TokenUrl => _settings.GetTokenUrl();

    public async Task<string> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        string? password = null,
        bool emailVerified = false,
        Dictionary<string, List<string>>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

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
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            } : null,
            requiredActions = password == null ? new[] { "UPDATE_PASSWORD" } : Array.Empty<string>()
        };

        var url = $"{AdminUrl}/users";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(userRepresentation, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException($"User with email {email} already exists in Keycloak");
        }

        response.EnsureSuccessStatusCode();

        var locationHeader = response.Headers.Location?.ToString();
        var keycloakUserId = locationHeader?.Split('/').LastOrDefault()
            ?? throw new InvalidOperationException("Failed to get Keycloak user ID from response");

        _logger.LogInformation("Created user {Email} in Keycloak with ID {KeycloakUserId}", email, keycloakUserId);

        return keycloakUserId;
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
        await EnsureAdminTokenAsync(cancellationToken);

        var updates = new Dictionary<string, object>();
        if (firstName != null) updates["firstName"] = firstName;
        if (lastName != null) updates["lastName"] = lastName;
        if (emailVerified.HasValue) updates["emailVerified"] = emailVerified.Value;
        if (enabled.HasValue) updates["enabled"] = enabled.Value;
        if (attributes != null) updates["attributes"] = attributes;

        var url = $"{AdminUrl}/users/{keycloakUserId}";
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(updates, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Updated user {KeycloakUserId} in Keycloak", keycloakUserId);
    }

    public async Task DeleteUserAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var url = $"{AdminUrl}/users/{keycloakUserId}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Deleted user {KeycloakUserId} from Keycloak", keycloakUserId);
    }

    public async Task<KeycloakUserDto?> GetUserByIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var url = $"{AdminUrl}/users/{keycloakUserId}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<KeycloakUserDto>(_jsonOptions, cancellationToken);
    }

    public async Task<KeycloakUserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var escapedEmail = Uri.EscapeDataString(email);
        var url = $"{AdminUrl}/users?email={escapedEmail}&exact=true";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(_jsonOptions, cancellationToken);
        return users?.FirstOrDefault();
    }

    public async Task AssignRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var roleUrl = $"{AdminUrl}/roles/{roleName}";
        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleUrl);
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        if (!roleResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Role {RoleName} not found in Keycloak", roleName);
            return;
        }

        var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(_jsonOptions, cancellationToken);
        if (role == null) return;

        var url = $"{AdminUrl}/users/{keycloakUserId}/role-mappings/realm";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new[] { role }, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Assigned role {RoleName} to user {KeycloakUserId}", roleName, keycloakUserId);
    }

    public async Task RemoveRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var roleUrl = $"{AdminUrl}/roles/{roleName}";
        var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleUrl);
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        if (!roleResponse.IsSuccessStatusCode) return;

        var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(_jsonOptions, cancellationToken);
        if (role == null) return;

        var url = $"{AdminUrl}/users/{keycloakUserId}/role-mappings/realm";
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(new[] { role }, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Removed role {RoleName} from user {KeycloakUserId}", roleName, keycloakUserId);
    }

    public async Task SendVerificationEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var url = $"{AdminUrl}/users/{keycloakUserId}/send-verify-email";
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Sent verification email to user {KeycloakUserId}", keycloakUserId);
    }

    public async Task SendPasswordResetEmailAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var url = $"{AdminUrl}/users/{keycloakUserId}/execute-actions-email";
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(new[] { "UPDATE_PASSWORD" })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Sent password reset email to user {KeycloakUserId}", keycloakUserId);
    }

    public async Task SetUserEnabledAsync(string keycloakUserId, bool enabled, CancellationToken cancellationToken = default)
    {
        await UpdateUserAsync(keycloakUserId, enabled: enabled, cancellationToken: cancellationToken);
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

        var response = await _httpClient.PostAsync(new Uri(TokenUrl), tokenRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get token for user {Email}: {StatusCode}", email, response.StatusCode);
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponseInternal>(_jsonOptions, cancellationToken);
        if (tokenResponse == null) return null;

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

    public async Task<KeycloakTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret ?? "",
            ["refresh_token"] = refreshToken
        });

        var response = await _httpClient.PostAsync(new Uri(TokenUrl), tokenRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to refresh token: {StatusCode}", response.StatusCode);
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponseInternal>(_jsonOptions, cancellationToken);
        if (tokenResponse == null) return null;

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

    public async Task LogoutAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var url = $"{AdminUrl}/users/{keycloakUserId}/logout";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Logged out user {KeycloakUserId} from all sessions", keycloakUserId);
    }

    private async Task EnsureAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (_adminToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
            return;

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret ?? ""
        });

        var response = await _httpClient.PostAsync(new Uri(TokenUrl), tokenRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponseInternal>(_jsonOptions, cancellationToken);
        
        _adminToken = tokenResponse?.AccessToken 
            ?? throw new InvalidOperationException("Failed to obtain admin token from Keycloak");
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        _logger.LogDebug("Obtained new admin token, expires at {Expiry}", _tokenExpiry);
    }

    private sealed class KeycloakTokenResponseInternal
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;
        
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }
        
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }

    private sealed class KeycloakRole
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
