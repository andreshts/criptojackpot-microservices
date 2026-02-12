namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Result of 2FA setup initiation.
/// Contains the secret and QR code URI for authenticator app setup.
/// </summary>
public class Setup2FaResultDto
{
    /// <summary>
    /// Base32-encoded secret for manual entry in authenticator apps.
    /// Only shown once during setup.
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    /// URI for QR code generation (otpauth://totp/...).
    /// Frontend should render this as a QR code.
    /// </summary>
    public string QrCodeUri { get; set; } = null!;
}

