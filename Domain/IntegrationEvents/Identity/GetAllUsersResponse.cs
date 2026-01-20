namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Response message containing the list of users.
/// Returned by Identity service in response to GetAllUsersRequest.
/// </summary>
public record GetAllUsersResponse
{
    public IEnumerable<UserInfo> Users { get; init; } = [];
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Basic user information for cross-service communication.
/// Contains only the necessary fields for notifications.
/// </summary>
public record UserInfo
{
    public Guid UserGuid { get; init; }
    public string Email { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string LastName { get; init; } = null!;
}
