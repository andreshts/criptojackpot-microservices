using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class UpdateUserImageCommandHandler : IRequestHandler<UpdateUserImageCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;

    public UpdateUserImageCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IStorageService storageService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<Result<UserDto>> Handle(
        UpdateUserImageCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        // Update the image path with the storage key
        user.ImagePath = request.StorageKey;
        
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = user.ToDto();
        
        // Return the presigned URL for immediate use
        userDto.ImagePath = _storageService.GetPresignedUrl(request.StorageKey);

        return Result.Ok(userDto);
    }
}

