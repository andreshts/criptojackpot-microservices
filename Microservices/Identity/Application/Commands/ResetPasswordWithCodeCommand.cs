using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class ResetPasswordWithCodeCommand : IRequest<Result<UserDto>>
{
    public string Email { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
