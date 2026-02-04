namespace CryptoJackpot.Identity.Application.Http;

/// <summary>
/// Internal DTO for deserializing Keycloak admin token responses.
/// </summary>
internal sealed class AdminTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
}
