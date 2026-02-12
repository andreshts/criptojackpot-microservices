using System.Security.Cryptography;

using CryptoJackpot.Identity.Application.Interfaces;
using OtpNet;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// TOTP service implementation using OtpNet library.
/// Requires NuGet package: Otp.NET
/// </summary>
public class TotpService : ITotpService
{
    private const int SecretLength = 20; // 160 bits - recommended for TOTP
    private const int CodeDigits = 6;
    private const int TimeStepSeconds = 30;
    
    public string GenerateSecret()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(SecretLength);
        return Base32Encoding.ToString(secretBytes);
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes, step: TimeStepSeconds, totpSize: CodeDigits);
            
            // Allow 1 step tolerance (30 seconds before/after) for clock drift
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "CryptoJackpot")
    {
        // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}&digits=6&period=30
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&digits={CodeDigits}&period={TimeStepSeconds}";
    }
}

