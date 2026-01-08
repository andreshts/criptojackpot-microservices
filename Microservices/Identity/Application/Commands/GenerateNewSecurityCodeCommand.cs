using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class GenerateNewSecurityCodeCommand : IRequest<Result<UserDto>>
{
    public long UserId { get; set; }
}
