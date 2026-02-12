namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Payload extracted from a validated Google ID token.
/// </summary>
public record GoogleUserPayload
{
    /// <summary>
    /// Google's unique identifier for the user ('sub' claim).
    /// This is immutable and should be used as the primary link.
    /// </summary>
    public required string GoogleId { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Whether Google has verified this email.
    /// </summary>
    public bool EmailVerified { get; init; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? GivenName { get; init; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? FamilyName { get; init; }

    /// <summary>
    /// User's full name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// URL to user's profile picture.
    /// </summary>
    public string? Picture { get; init; }
}

