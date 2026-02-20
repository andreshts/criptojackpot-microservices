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

public class GetUserByIdQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly GetUserByIdQueryHandler _sut;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _storageService = Substitute.For<IStorageService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetUserByIdQueryHandler(_userRepository, _storageService, _mapper);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static User CreateUser(long id = 1) => new()
    {
        Id = id,
        UserGuid = Guid.NewGuid(),
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
        _userRepository.GetByIdAsync(Arg.Any<long>()).Returns((User?)null);
        var query = new GetUserByIdQuery { UserId = 999 };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Be("User not found");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: User found → DTO with presigned image URL
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserFound_ReturnsDtoWithPresignedImageUrl()
    {
        // Arrange
        const string rawPath = "users/1/avatar.jpg";
        const string presignedUrl = "https://cdn.example.com/signed/avatar.jpg";

        var user = CreateUser();
        var dto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName, ImagePath = rawPath };

        _userRepository.GetByIdAsync(1).Returns(user);
        _mapper.Map<UserDto>(user).Returns(dto);
        _storageService.GetImageUrl(rawPath).Returns(presignedUrl);

        var query = new GetUserByIdQuery { UserId = 1 };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImagePath.Should().Be(presignedUrl);
        result.Value.Email.Should().Be(user.Email);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Correct UserId is queried
    // ═════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(9999)]
    public async Task Handle_CallsRepositoryWithCorrectId(long userId)
    {
        // Arrange
        var user = CreateUser(userId);
        var dto = new UserDto { Id = userId, Email = user.Email, Name = user.Name, LastName = user.LastName };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _mapper.Map<UserDto>(user).Returns(dto);
        _storageService.GetImageUrl(Arg.Any<string?>()).Returns((string?)null);

        var query = new GetUserByIdQuery { UserId = userId };

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).GetByIdAsync(userId);
    }
}

