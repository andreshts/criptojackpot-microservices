namespace CryptoJackpot.Domain.Core.Enums;

/// <summary>
/// Types of security alerts that can be triggered by the Identity service.
/// </summary>
public enum SecurityAlertType
{
    /// <summary>
    /// A previously rotated refresh token was reused (potential token theft).
    /// </summary>
    RefreshTokenReuse = 1,
    
    /// <summary>
    /// Login from a new device/location.
    /// </summary>
    NewDeviceLogin = 2,
    
    /// <summary>
    /// Multiple failed 2FA attempts.
    /// </summary>
    TwoFactorBruteForce = 3,
    
    /// <summary>
    /// Password was changed.
    /// </summary>
    PasswordChanged = 4,
    
    /// <summary>
    /// 2FA was disabled.
    /// </summary>
    TwoFactorDisabled = 5,
    
    /// <summary>
    /// Recovery code was used.
    /// </summary>
    RecoveryCodeUsed = 6
}

