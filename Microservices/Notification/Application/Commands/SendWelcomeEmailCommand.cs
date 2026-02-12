using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

/// <summary>
/// Command to send a welcome email to new users (Google OAuth registrations).
/// </summary>
public class SendWelcomeEmailCommand : IRequest<Result<bool>>
{
    public long UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

