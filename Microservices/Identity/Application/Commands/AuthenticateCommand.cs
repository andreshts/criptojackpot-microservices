using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class AuthenticateCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
