using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class UpdateUserImageCommand : IRequest<Result<UserDto>>
{
    public long UserId { get; set; }
    public string StorageKey { get; set; } = null!;
}

