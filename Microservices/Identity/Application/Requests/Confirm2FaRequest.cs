namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request to confirm 2FA setup.
/// </summary>
public class Confirm2FaRequest
{
    /// <summary>
    /// 6-digit TOTP code from authenticator app.
    /// </summary>
    public string Code { get; set; } = null!;
}

