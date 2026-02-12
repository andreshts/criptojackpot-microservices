namespace CryptoJackpot.Identity.Application.DTOs;
/// <summary>
/// Internal result from login command containing tokens.
/// Tokens are NOT returned to the client - they are set as HttpOnly cookies by the controller.
/// </summary>
public class LoginResultDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresInMinutes { get; set; }
    public AuthResponseDto User { get; set; } = null!;
    public bool RequiresTwoFactor { get; set; }
}
