using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IStorageService storageService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        var userDto = _mapper.Map<UserDto>(user);
        
        // Generate presigned URL for image if exists
        if (!string.IsNullOrEmpty(userDto.ImagePath))
            userDto.ImagePath = _storageService.GetPresignedUrl(userDto.ImagePath);

        return Result.Ok(userDto);
    }
}
