using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Queries;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Queries;

public class GetAllUsersQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly GetAllUsersQueryHandler _sut;

    public GetAllUsersQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _storageService = Substitute.For<IStorageService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetAllUsersQueryHandler(_userRepository, _storageService, _mapper);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static User CreateUser(long id, string imagePath = "users/avatar.jpg") => new()
    {
        Id = id,
        UserGuid = Guid.NewGuid(),
        Email = $"user{id}@test.com",
        Name = $"User{id}",
        LastName = "Doe",
        ImagePath = imagePath
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Empty user list → returns empty enumerable
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NoUsers_ReturnsEmptyList()
    {
        // Arrange
        _userRepository.GetAllAsync(Arg.Any<long?>()).Returns(Enumerable.Empty<User>());
        var query = new GetAllUsersQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Users found → each DTO gets presigned image URL
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UsersExist_MapsDtosWithPresignedImageUrls()
    {
        // Arrange
        var users = new[]
        {
            CreateUser(1, "users/1/avatar.jpg"),
            CreateUser(2, "users/2/avatar.png")
        };

        var dto1 = new UserDto { Id = 1, Email = users[0].Email, Name = users[0].Name, LastName = users[0].LastName, ImagePath = users[0].ImagePath };
        var dto2 = new UserDto { Id = 2, Email = users[1].Email, Name = users[1].Name, LastName = users[1].LastName, ImagePath = users[1].ImagePath };

        _userRepository.GetAllAsync(Arg.Any<long?>()).Returns(users);
        _mapper.Map<UserDto>(users[0]).Returns(dto1);
        _mapper.Map<UserDto>(users[1]).Returns(dto2);
        _storageService.GetImageUrl("users/1/avatar.jpg").Returns("https://cdn.example.com/1/avatar.jpg");
        _storageService.GetImageUrl("users/2/avatar.png").Returns("https://cdn.example.com/2/avatar.png");

        var query = new GetAllUsersQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dtos = result.Value.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].ImagePath.Should().Be("https://cdn.example.com/1/avatar.jpg");
        dtos[1].ImagePath.Should().Be("https://cdn.example.com/2/avatar.png");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: ExcludeUserId is passed to repository
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithExcludeUserId_PassesCorrectIdToRepository()
    {
        // Arrange
        _userRepository.GetAllAsync(Arg.Any<long?>()).Returns(Enumerable.Empty<User>());
        var query = new GetAllUsersQuery { ExcludeUserId = 42 };

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).GetAllAsync(42);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: No ExcludeUserId → passes null to repository
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithoutExcludeUserId_PassesNullToRepository()
    {
        // Arrange
        _userRepository.GetAllAsync(Arg.Any<long?>()).Returns(Enumerable.Empty<User>());
        var query = new GetAllUsersQuery { ExcludeUserId = null };

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).GetAllAsync();
    }
}

