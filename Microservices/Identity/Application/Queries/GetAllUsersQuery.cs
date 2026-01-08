using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetAllUsersQuery : IRequest<Result<IEnumerable<UserDto>>>
{
    public long? ExcludeUserId { get; set; }
}
