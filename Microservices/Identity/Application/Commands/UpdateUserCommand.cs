using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class UpdateUserCommand : IRequest<Result<UserDto>>
{
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Password { get; set; }
}
