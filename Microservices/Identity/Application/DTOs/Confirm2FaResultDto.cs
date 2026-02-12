namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Result of 2FA confirmation/activation.
/// Contains recovery codes that must be shown to user only once.
/// </summary>
public class Confirm2FaResultDto
{
    /// <summary>
    /// Recovery codes for backup access if authenticator is lost.
    /// Format: XXXX-XXXX. Show ONLY ONCE - they are hashed after this.
    /// </summary>
    public IReadOnlyList<string> RecoveryCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Number of recovery codes generated.
    /// </summary>
    public int RecoveryCodeCount => RecoveryCodes.Count;
}

