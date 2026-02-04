using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoJackpot.Identity.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Http;

/// <summary>
/// A delegating handler that automatically attaches the Keycloak admin Bearer token
/// to all outgoing HTTP requests and handles token refresh when expired.
/// This keeps the token management logic separate from business logic.
/// </summary>
public class KeycloakAdminTokenHandler : DelegatingHandler
{
    private const string BearerScheme = "Bearer";
    
    private readonly IOptions<KeycloakSettings> _settings;
    private readonly ILogger<KeycloakAdminTokenHandler> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _adminToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public KeycloakAdminTokenHandler(
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakAdminTokenHandler> logger)
    {
        _settings = settings;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        if (IsAdminApiRequest(request.RequestUri))
        {
            await EnsureAdminTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && IsAdminApiRequest(request.RequestUri))
        {
            _logger.LogWarning("Received 401 from Keycloak Admin API, attempting token refresh");
            _tokenExpiry = DateTime.MinValue;
            await EnsureAdminTokenAsync(cancellationToken);
            
            var retryRequest = await CloneHttpRequestMessageAsync(request);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, _adminToken);
            
            response = await base.SendAsync(retryRequest, cancellationToken);
        }

        return response;
    }

    private bool IsAdminApiRequest(Uri? uri)
    {
        if (uri == null) return false;
        var adminPath = $"/admin/realms/{_settings.Value.Realm}";
        return uri.AbsolutePath.Contains(adminPath, StringComparison.OrdinalIgnoreCase);
    }

    private async Task EnsureAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (_adminToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
            return;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_adminToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
                return;

            await RefreshAdminTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task RefreshAdminTokenAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.Value;
        var tokenUrl = settings.GetTokenUrl();

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret ?? ""
        });

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl) { Content = tokenRequest };
        var response = await base.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<AdminTokenResponse>(_jsonOptions, cancellationToken);
        
        _adminToken = tokenResponse?.AccessToken 
            ?? throw new InvalidOperationException("Failed to obtain admin token from Keycloak");
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        _logger.LogDebug("Obtained new admin token, expires at {Expiry}", _tokenExpiry);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
