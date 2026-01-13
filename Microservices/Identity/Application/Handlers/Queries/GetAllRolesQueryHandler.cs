using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public GetAllRolesQueryHandler(
        IRoleRepository roleRepository,
        IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(
        GetAllRolesQuery request, 
        CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllRoles();
        var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);

        return Result.Ok(roleDtos);
    }
}

