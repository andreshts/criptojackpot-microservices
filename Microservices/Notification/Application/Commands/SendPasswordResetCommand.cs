using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

public class SendPasswordResetCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
}
