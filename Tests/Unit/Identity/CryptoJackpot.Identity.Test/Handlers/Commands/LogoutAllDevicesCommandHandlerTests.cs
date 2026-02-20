using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Responses.Errors;
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

public class LogoutAllDevicesCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LogoutAllDevicesCommandHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public LogoutAllDevicesCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _eventPublisher = Substitute.For<IIdentityEventPublisher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<LogoutAllDevicesCommandHandler>>();

        _sut = new LogoutAllDevicesCommandHandler(
            _userRepository,
            _refreshTokenRepository,
            _eventPublisher,
            _unitOfWork,
            logger);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static User CreateUser() => new()
    {
        Id = 1,
        UserGuid = UserGuid,
        Email = "user@cryptojackpot.com",
        Name = "John",
        LastName = "Doe"
    };

    private static LogoutAllDevicesCommand CreateCommand(string? reason = null) => new()
    {
        UserGuid = UserGuid,
        Reason = reason
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → NotFoundError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository.GetByGuidAsync(UserGuid).Returns((User?)null);
        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>();

        await _refreshTokenRepository.DidNotReceive()
            .RevokeAllByUserIdAsync(Arg.Any<long>(), Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: No active sessions → returns 0 without revoking
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NoActiveSessions_ReturnsZeroWithoutRevoking()
    {
        // Arrange
        var user = CreateUser();
        _userRepository.GetByGuidAsync(UserGuid).Returns(user);
        _refreshTokenRepository.GetActiveByUserIdAsync(user.Id)
            .Returns(new List<UserRefreshToken>().AsReadOnly());

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);

        await _refreshTokenRepository.DidNotReceive()
            .RevokeAllByUserIdAsync(Arg.Any<long>(), Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Active sessions exist → revoked and count returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ActiveSessionsExist_RevokesAllAndReturnsCount()
    {
        // Arrange
        var user = CreateUser();
        var activeSessions = new List<UserRefreshToken>
        {
            new() { Id = 1, UserId = user.Id },
            new() { Id = 2, UserId = user.Id },
            new() { Id = 3, UserId = user.Id }
        };

        _userRepository.GetByGuidAsync(UserGuid).Returns(user);
        _refreshTokenRepository.GetActiveByUserIdAsync(user.Id).Returns(activeSessions);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3);

        await _refreshTokenRepository.Received(1).RevokeAllByUserIdAsync(user.Id, Arg.Any<string>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Security alert published after revoking sessions
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ActiveSessionsRevoked_PublishesSecurityAlert()
    {
        // Arrange
        var user = CreateUser();
        var activeSessions = new List<UserRefreshToken>
        {
            new() { Id = 1, UserId = user.Id }
        };

        _userRepository.GetByGuidAsync(UserGuid).Returns(user);
        _refreshTokenRepository.GetActiveByUserIdAsync(user.Id).Returns(activeSessions);

        var command = CreateCommand();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishSecurityAlertAsync(
            Arg.Is<User>(u => u == user),
            Arg.Is<SecurityAlertType>(a => a == SecurityAlertType.AllSessionsRevoked),
            Arg.Any<string>(),
            Arg.Is<string?>(x => x == null),
            Arg.Is<string?>(x => x == null));
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: Custom reason is used; fallback reason when null
    // ═════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("suspected_compromise", "suspected_compromise")]
    [InlineData(null, "logout_all_devices")]
    [InlineData("", "logout_all_devices")]
    public async Task Handle_Reason_UsesCustomOrFallback(string? inputReason, string expectedReason)
    {
        // Arrange
        var user = CreateUser();
        var activeSessions = new List<UserRefreshToken> { new() { Id = 1, UserId = user.Id } };

        _userRepository.GetByGuidAsync(UserGuid).Returns(user);
        _refreshTokenRepository.GetActiveByUserIdAsync(user.Id).Returns(activeSessions);

        var command = CreateCommand(reason: inputReason);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1).RevokeAllByUserIdAsync(user.Id, expectedReason);
    }
}

