namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Root configuration object for Identity service.
/// </summary>
public class ApplicationConfiguration
{
    public JwtConfig? JwtSettings { get; init; }
    public GoogleAuthConfig? GoogleAuth { get; init; }
    public TwoFactorConfig? TwoFactor { get; init; }
}
