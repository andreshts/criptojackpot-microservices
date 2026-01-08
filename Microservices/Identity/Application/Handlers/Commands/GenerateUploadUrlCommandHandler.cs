using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class GenerateUploadUrlCommandHandler : IRequestHandler<GenerateUploadUrlCommand, Result<UploadUrlDto>>
{
    private readonly IStorageService _storageService;
    private readonly IUserRepository _userRepository;

    public GenerateUploadUrlCommandHandler(
        IStorageService storageService,
        IUserRepository userRepository)
    {
        _storageService = storageService;
        _userRepository = userRepository;
    }

    public async Task<Result<UploadUrlDto>> Handle(
        GenerateUploadUrlCommand request, 
        CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Fail<UploadUrlDto>(new NotFoundError("User not found"));

        // Validate file extension
        if (!_storageService.IsValidFileExtension(request.FileName))
            return Result.Fail<UploadUrlDto>(new BadRequestError("Invalid file type. Allowed extensions: .jpg, .jpeg, .png, .gif, .webp"));

        // Generate presigned upload URL
        var (url, key) = _storageService.GeneratePresignedUploadUrl(
            request.UserId,
            request.FileName,
            request.ContentType,
            request.ExpirationMinutes);

        var response = new UploadUrlDto
        {
            UploadUrl = url,
            StorageKey = key
        };

        return Result.Ok(response);
    }
}

