using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class GenerateUploadUrlCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IStorageService _storageService;
    private readonly IUserRepository _userRepository;
    private readonly GenerateUploadUrlCommandHandler _sut;

    public GenerateUploadUrlCommandHandlerTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new GenerateUploadUrlCommandHandler(_storageService, _userRepository);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static GenerateUploadUrlCommand CreateCommand(
        long userId = 1,
        string fileName = "avatar.jpg",
        string contentType = "image/jpeg",
        int expirationMinutes = 15)
    {
        return new GenerateUploadUrlCommand
        {
            UserId = userId,
            FileName = fileName,
            ContentType = contentType,
            ExpirationMinutes = expirationMinutes
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → NotFoundError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<long>()).Returns((User?)null);
        var command = CreateCommand(userId: 999);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>();

        _storageService.DidNotReceive().GeneratePresignedUploadUrl(
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Invalid file extension → BadRequestError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidFileExtension_ReturnsBadRequestError()
    {
        // Arrange
        var user = new User { Id = 1, Email = "user@test.com", Name = "John", LastName = "Doe" };
        _userRepository.GetByIdAsync(1).Returns(user);
        _storageService.IsValidFileExtension("malicious.exe").Returns(false);

        var command = CreateCommand(fileName: "malicious.exe");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<BadRequestError>()
            .Which.Message.Should().Contain("Invalid file type");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Valid user and extension → upload URL generated
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPresignedUploadUrl()
    {
        // Arrange
        const string expectedUrl = "https://cdn.digitalocean.com/presigned/upload?token=xyz";
        const string expectedKey = "users/1/avatars/avatar.jpg";

        var user = new User { Id = 1, Email = "user@test.com", Name = "John", LastName = "Doe" };
        _userRepository.GetByIdAsync(1).Returns(user);
        _storageService.IsValidFileExtension("avatar.jpg").Returns(true);
        _storageService.GeneratePresignedUploadUrl(1, "avatar.jpg", "image/jpeg")
            .Returns((expectedUrl, expectedKey));

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UploadUrl.Should().Be(expectedUrl);
        result.Value.StorageKey.Should().Be(expectedKey);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Correct arguments passed to storage service
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_CallsStorageServiceWithCorrectArguments()
    {
        // Arrange
        var user = new User { Id = 5, Email = "user@test.com", Name = "John", LastName = "Doe" };
        _userRepository.GetByIdAsync(5).Returns(user);
        _storageService.IsValidFileExtension(Arg.Any<string>()).Returns(true);
        _storageService.GeneratePresignedUploadUrl(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(("url", "key"));

        var command = CreateCommand(userId: 5, fileName: "photo.png", contentType: "image/png", expirationMinutes: 30);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _storageService.Received(1).GeneratePresignedUploadUrl(5, "photo.png", "image/png", 30);
    }
}

