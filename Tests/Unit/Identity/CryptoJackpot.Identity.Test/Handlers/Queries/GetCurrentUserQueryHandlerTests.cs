using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Queries;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Queries;

public class GetCurrentUserQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly GetCurrentUserQueryHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public GetCurrentUserQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _storageService = Substitute.For<IStorageService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetCurrentUserQueryHandler(_userRepository, _storageService, _mapper);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static User CreateUser() => new()
    {
        Id = 1,
        UserGuid = UserGuid,
        Email = "user@cryptojackpot.com",
        Name = "John",
        LastName = "Doe",
        ImagePath = "users/1/avatar.jpg"
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → NotFoundError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository.GetByGuidAsync(UserGuid).Returns((User?)null);
        var query = new GetCurrentUserQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Be("User not found");

        _storageService.DidNotReceive().GetImageUrl(Arg.Any<string?>(), Arg.Any<int>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: User found → DTO mapped with presigned image URL
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserFound_ReturnsDtoWithPresignedImageUrl()
    {
        // Arrange
        const string rawImagePath = "users/1/avatar.jpg";
        const string presignedUrl = "https://cdn.example.com/users/1/avatar.jpg?token=xyz";

        var user = CreateUser();
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName, ImagePath = rawImagePath };

        _userRepository.GetByGuidAsync(UserGuid).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);
        _storageService.GetImageUrl(rawImagePath).Returns(presignedUrl);

        var query = new GetCurrentUserQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImagePath.Should().Be(presignedUrl, "image path should be presigned URL");
        result.Value.Email.Should().Be(user.Email);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: User found with null ImagePath → GetImageUrl still called
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserWithNullImagePath_CallsGetImageUrlWithNull()
    {
        // Arrange
        var user = CreateUser();
        user.ImagePath = null;
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName, ImagePath = null };

        _userRepository.GetByGuidAsync(UserGuid).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);
        _storageService.GetImageUrl(null).Returns((string?)null);

        var query = new GetCurrentUserQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _storageService.Received(1).GetImageUrl(null);
    }
}

