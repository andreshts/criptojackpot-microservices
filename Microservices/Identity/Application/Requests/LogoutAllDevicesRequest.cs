namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request to logout from all devices.
/// </summary>
public class LogoutAllDevicesRequest
{
    /// <summary>
    /// Optional reason for logout (e.g., "suspected_compromise", "password_changed").
    /// </summary>
    public string? Reason { get; set; }
}

