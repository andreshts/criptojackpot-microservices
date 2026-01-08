using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class ConfirmEmailCommand : IRequest<Result<string>>
{
    public string Token { get; set; } = null!;
}
