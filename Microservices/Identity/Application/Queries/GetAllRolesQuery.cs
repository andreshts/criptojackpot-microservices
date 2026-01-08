using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetAllRolesQuery : IRequest<Result<IEnumerable<RoleDto>>>
{
}

