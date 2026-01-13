using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository,
        IStorageService storageService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(request.ExcludeUserId);
        var userDtos = users.Select(u =>
        {
            var dto = _mapper.Map<UserDto>(u);
            if (!string.IsNullOrEmpty(dto.ImagePath))
                dto.ImagePath = _storageService.GetPresignedUrl(dto.ImagePath);
            return dto;
        });

        return Result.Ok(userDtos);
    }
}