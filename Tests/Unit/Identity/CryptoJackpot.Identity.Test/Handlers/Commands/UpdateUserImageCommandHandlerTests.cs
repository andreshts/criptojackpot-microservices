using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class UpdateUserImageCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly UpdateUserImageCommandHandler _sut;

    public UpdateUserImageCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _storageService = Substitute.For<IStorageService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new UpdateUserImageCommandHandler(
            _userRepository,
            _unitOfWork,
            _storageService,
            _mapper);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static User CreateUser(long id = 1) => new()
    {
        Id = id,
        UserGuid = Guid.NewGuid(),
        Email = "user@test.com",
        Name = "John",
        LastName = "Doe"
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → NotFoundError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<long>()).Returns((User?)null);
        var command = new UpdateUserImageCommand { UserId = 999, StorageKey = "users/999/avatar.jpg" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>();

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: User exists → ImagePath updated and saved
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserExists_UpdatesImagePathAndSaves()
    {
        // Arrange
        const string storageKey = "users/1/avatars/new_avatar.png";
        var user = CreateUser();
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName };

        _userRepository.GetByIdAsync(1).Returns(user);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);
        _storageService.GetImageUrl(storageKey).Returns("https://cdn.example.com/presigned");

        var command = new UpdateUserImageCommand { UserId = 1, StorageKey = storageKey };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify: image path stored on entity
        user.ImagePath.Should().Be(storageKey);

        // Verify: persisted
        await _userRepository.Received(1).UpdateAsync(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Returned DTO contains presigned URL (not raw key)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserExists_ReturnsDtoWithPresignedUrl()
    {
        // Arrange
        const string storageKey = "users/1/avatars/avatar.jpg";
        const string presignedUrl = "https://cdn.example.com/users/1/avatars/avatar.jpg?token=abc";

        var user = CreateUser();
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName, ImagePath = storageKey };

        _userRepository.GetByIdAsync(1).Returns(user);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);
        _storageService.GetImageUrl(storageKey).Returns(presignedUrl);

        var command = new UpdateUserImageCommand { UserId = 1, StorageKey = storageKey };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImagePath.Should().Be(presignedUrl, "DTO should expose presigned URL, not raw storage key");
    }
}

