namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Configuration for Two-Factor Authentication (TOTP).
/// Bind to "TwoFactor" section in appsettings.json.
/// </summary>
public class TwoFactorConfig
{
    public const string SectionName = "TwoFactor";
    
    /// <summary>
    /// Issuer name shown in authenticator apps (e.g., "CryptoJackpot").
    /// </summary>
    public string Issuer { get; set; } = "CryptoJackpot";
    
    /// <summary>
    /// Challenge token TTL in minutes for 2FA verification flow.
    /// Default: 5 minutes.
    /// </summary>
    public int ChallengeTokenMinutes { get; set; } = 5;
    
    /// <summary>
    /// Number of recovery codes to generate when enabling 2FA.
    /// Default: 8 codes.
    /// </summary>
    public int RecoveryCodeCount { get; set; } = 8;
}

