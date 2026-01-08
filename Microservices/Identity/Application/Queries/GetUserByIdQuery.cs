using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public long UserId { get; set; }
}
