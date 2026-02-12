namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for TOTP (Time-based One-Time Password) operations.
/// Used for 2FA setup and verification.
/// </summary>
public interface ITotpService
{
    /// <summary>
    /// Generates a new Base32-encoded secret for TOTP setup.
    /// </summary>
    string GenerateSecret();

    /// <summary>
    /// Validates a TOTP code against the secret.
    /// </summary>
    /// <param name="secret">Base32-encoded secret</param>
    /// <param name="code">6-digit code from authenticator app</param>
    /// <returns>True if code is valid</returns>
    bool ValidateCode(string secret, string code);

    /// <summary>
    /// Generates a URI for QR code scanning in authenticator apps.
    /// Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}
    /// </summary>
    string GenerateQrCodeUri(string email, string secret, string issuer = "CryptoJackpot");
}

