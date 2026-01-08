using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(
        GetAllRolesQuery request, 
        CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllRoles();

        var roleDtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        });

        return Result.Ok(roleDtos);
    }
}

