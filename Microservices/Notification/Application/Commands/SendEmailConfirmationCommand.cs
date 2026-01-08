using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

public class SendEmailConfirmationCommand : IRequest<Result<bool>>
{
    public long UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Token { get; set; } = null!;
}
