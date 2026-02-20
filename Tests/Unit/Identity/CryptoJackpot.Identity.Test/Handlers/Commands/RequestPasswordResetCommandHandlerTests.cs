using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class RequestPasswordResetCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly RequestPasswordResetCommandHandler _sut;

    public RequestPasswordResetCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _eventPublisher = Substitute.For<IIdentityEventPublisher>();
        var logger = Substitute.For<ILogger<RequestPasswordResetCommandHandler>>();

        _sut = new RequestPasswordResetCommandHandler(
            _userRepository,
            _passwordHasher,
            _unitOfWork,
            _eventPublisher,
            logger);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static User CreateUser(
        string email = "user@cryptojackpot.com",
        bool hasPassword = true,
        DateTime? resetTokenExpiresAt = null)
    {
        return new User
        {
            Id = 1,
            Email = email,
            Name = "John",
            LastName = "Doe",
            PasswordHash = hasPassword ? "hashed_password" : null,
            PasswordResetTokenExpiresAt = resetTokenExpiresAt
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → returns Ok(true) (anti-enumeration)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsOkTrueWithoutPublishing()
    {
        // Arrange
        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns((User?)null);
        var command = new RequestPasswordResetCommand { Email = "nonexistent@test.com" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert (anti-enumeration: always returns true)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        await _eventPublisher.DidNotReceive()
            .PublishPasswordResetRequestedAsync(Arg.Any<User>(), Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Google-only user (no password) → returns Ok(true)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_GoogleOnlyUser_ReturnsOkTrueWithoutPublishing()
    {
        // Arrange
        var user = CreateUser(hasPassword: false);
        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        var command = new RequestPasswordResetCommand { Email = user.Email };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        await _eventPublisher.DidNotReceive()
            .PublishPasswordResetRequestedAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Request throttled (token still valid within cooldown)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ThrottledRequest_ReturnsOkTrueWithoutPublishing()
    {
        // Arrange: token expires far in the future (above 15-60s = 14m+ remaining)
        var user = CreateUser(resetTokenExpiresAt: DateTime.UtcNow.AddMinutes(15));
        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        var command = new RequestPasswordResetCommand { Email = user.Email };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        await _eventPublisher.DidNotReceive()
            .PublishPasswordResetRequestedAsync(Arg.Any<User>(), Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Valid user, no previous token → sends reset email
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidUserNoPreviousToken_SavesAndPublishesEvent()
    {
        // Arrange
        var user = CreateUser(resetTokenExpiresAt: null);
        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_code");

        var command = new RequestPasswordResetCommand { Email = user.Email };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1)
            .PublishPasswordResetRequestedAsync(user, Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: Email is normalized (trimmed + lowercased) before lookup
    // ═════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("  USER@Test.COM  ", "user@test.com")]
    [InlineData("Admin@Example.COM", "admin@example.com")]
    public async Task Handle_NormalizesEmailBeforeLookup(string inputEmail, string expectedNormalized)
    {
        // Arrange
        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns((User?)null);
        var command = new RequestPasswordResetCommand { Email = inputEmail };

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert: lookup was called with normalized email
        await _userRepository.Received(1).GetByEmailCaseInsensitiveAsync(expectedNormalized);
    }
}

