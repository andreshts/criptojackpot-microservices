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
/// Implementation of IKeycloakRoleService for role management operations.
/// Token management is handled by KeycloakAdminTokenHandler.
/// </summary>
public class KeycloakRoleService : IKeycloakRoleService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakRoleService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KeycloakRoleService(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakRoleService> logger)
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

    public async Task AssignRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default)
    {
        var role = await GetRealmRoleAsync(roleName, cancellationToken);
        if (role == null)
        {
            _logger.LogWarning("Role {RoleName} not found in Keycloak", roleName);
            return;
        }

        var url = $"{AdminUrl}{KeycloakEndpoints.Users.RoleMappingsRealm(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new[] { role }, options: _jsonOptions)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Assigned role {RoleName} to user {KeycloakUserId}", roleName, keycloakUserId);
    }

    public async Task RemoveRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default)
    {
        var role = await GetRealmRoleAsync(roleName, cancellationToken);
        if (role == null) return;

        var url = $"{AdminUrl}{KeycloakEndpoints.Users.RoleMappingsRealm(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(new[] { role }, options: _jsonOptions)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Removed role {RoleName} from user {KeycloakUserId}", roleName, keycloakUserId);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Users.RoleMappingsRealm(keycloakUserId)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRole>>(_jsonOptions, cancellationToken);
        return roles?.Select(r => r.Name).ToList() ?? new List<string>();
    }

    public async Task<bool> HasRoleAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default)
    {
        var roles = await GetUserRolesAsync(keycloakUserId, cancellationToken);
        return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<KeycloakRole?> GetRealmRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var url = $"{AdminUrl}{KeycloakEndpoints.Roles.ByName(roleName)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<KeycloakRole>(_jsonOptions, cancellationToken);
    }
}
