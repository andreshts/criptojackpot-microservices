namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request DTO for user login.
/// Validation is handled by LoginCommandValidator via FluentValidation.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    
    /// <summary>
    /// If true, extends the refresh token expiration to 30 days.
    /// </summary>
    public bool RememberMe { get; set; }
}

