using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class UpdatePasswordCommand : IRequest<Result<UserDto>>
{
    public long UserId { get; set; }
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
