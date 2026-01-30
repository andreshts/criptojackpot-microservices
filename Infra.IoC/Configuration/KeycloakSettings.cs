namespace CryptoJackpot.Infra.IoC.Configuration;

/// <summary>
/// Configuration settings for Keycloak OIDC authentication.
/// </summary>
public class KeycloakSettings
{
    /// <summary>
    /// The Keycloak server URL (e.g., http://localhost:8180)
    /// </summary>
    public string Authority { get; set; } = null!;
    
    /// <summary>
    /// The Keycloak realm name (e.g., cryptojackpot)
    /// </summary>
    public string Realm { get; set; } = null!;
    
    /// <summary>
    /// The client ID for this microservice (e.g., cryptojackpot-backend)
    /// </summary>
    public string ClientId { get; set; } = null!;
    
    /// <summary>
    /// The client secret for confidential clients
    /// </summary>
    public string? ClientSecret { get; set; }
    
    /// <summary>
    /// The expected audience for token validation
    /// </summary>
    public string? Audience { get; set; }
    
    /// <summary>
    /// Whether to require HTTPS for metadata retrieval (disable for local development)
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;
    
    /// <summary>
    /// Whether to validate the audience claim
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Gets the full authority URL including the realm.
    /// </summary>
    public string GetRealmUrl() => $"{Authority.TrimEnd('/')}/realms/{Realm}";
    
    /// <summary>
    /// Gets the OIDC well-known configuration URL.
    /// </summary>
    public string GetWellKnownUrl() => $"{GetRealmUrl()}/.well-known/openid-configuration";
    
    /// <summary>
    /// Gets the token endpoint URL.
    /// </summary>
    public string GetTokenUrl() => $"{GetRealmUrl()}/protocol/openid-connect/token";
    
    /// <summary>
    /// Gets the userinfo endpoint URL.
    /// </summary>
    public string GetUserInfoUrl() => $"{GetRealmUrl()}/protocol/openid-connect/userinfo";
    
    /// <summary>
    /// Gets the logout endpoint URL.
    /// </summary>
    public string GetLogoutUrl() => $"{GetRealmUrl()}/protocol/openid-connect/logout";
    
    /// <summary>
    /// Gets the Admin API URL for user management.
    /// </summary>
    public string GetAdminUrl() => $"{Authority.TrimEnd('/')}/admin/realms/{Realm}";
}
