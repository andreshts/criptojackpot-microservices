namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request to disable 2FA.
/// </summary>
public class Disable2FaRequest
{
    /// <summary>
    /// 6-digit TOTP code from authenticator app.
    /// Either Code or RecoveryCode must be provided.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Recovery code if authenticator is unavailable.
    /// </summary>
    public string? RecoveryCode { get; set; }
}

