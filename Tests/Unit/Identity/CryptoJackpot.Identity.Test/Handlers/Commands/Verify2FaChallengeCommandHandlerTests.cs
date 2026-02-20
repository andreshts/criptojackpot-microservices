using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class Verify2FaChallengeCommandHandlerTests
{
    private readonly ITwoFactorVerificationService _twoFactorService;
    private readonly Verify2FaChallengeCommandHandler _sut;

    public Verify2FaChallengeCommandHandlerTests()
    {
        _twoFactorService = Substitute.For<ITwoFactorVerificationService>();
        _sut = new Verify2FaChallengeCommandHandler(_twoFactorService);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static Verify2FaChallengeCommand CreateCommand(
        string challengeToken = "challenge_jwt",
        string? code = "123456",
        string? recoveryCode = null)
    {
        return new Verify2FaChallengeCommand
        {
            ChallengeToken = challengeToken,
            Code = code,
            RecoveryCode = recoveryCode,
            DeviceInfo = "Chrome/120",
            IpAddress = "127.0.0.1",
            RememberMe = false
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid TOTP code → login completed with tokens
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidTotpCode_ReturnsLoginResultWithTokens()
    {
        // Arrange
        var loginResult = new LoginResultDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresInMinutes = 15,
            RequiresTwoFactor = false,
            User = new AuthResponseDto { Email = "user@test.com", Name = "John", LastName = "Doe" }
        };

        _twoFactorService
            .VerifyChallengeAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(loginResult));

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RequiresTwoFactor.Should().BeFalse();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Invalid/expired challenge token → failure
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidChallengeToken_PropagatesError()
    {
        // Arrange
        _twoFactorService
            .VerifyChallengeAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<LoginResultDto>("Invalid or expired challenge token"));

        var command = CreateCommand(challengeToken: "expired_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Recovery code used instead of TOTP → succeeds
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_RecoveryCodeProvided_DelegatesCorrectly()
    {
        // Arrange
        const string recoveryCode = "ABCD-EFGH";
        var loginResult = new LoginResultDto
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            ExpiresInMinutes = 15,
            User = new AuthResponseDto { Email = "user@test.com", Name = "John", LastName = "Doe" }
        };

        _twoFactorService
            .VerifyChallengeAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(loginResult));

        var command = CreateCommand(code: null, recoveryCode: recoveryCode);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert: recovery code passed as recoveryCode argument
        await _twoFactorService.Received(1).VerifyChallengeAsync(
            Arg.Any<string>(),
            Arg.Is<string?>(x => x == null),
            Arg.Is<string?>(x => x == recoveryCode),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: All command parameters correctly delegated
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_DelegatesAllArgumentsToService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var loginResult = new LoginResultDto
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            ExpiresInMinutes = 15,
            User = new AuthResponseDto { Email = "user@test.com", Name = "John", LastName = "Doe" }
        };

        _twoFactorService
            .VerifyChallengeAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(loginResult));

        var command = new Verify2FaChallengeCommand
        {
            ChallengeToken = "challenge_jwt",
            Code = "654321",
            RecoveryCode = null,
            DeviceInfo = "Safari/17",
            IpAddress = "10.0.0.1",
            RememberMe = true
        };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _twoFactorService.Received(1).VerifyChallengeAsync(
            Arg.Is<string>(x => x == "challenge_jwt"),
            Arg.Is<string?>(x => x == "654321"),
            Arg.Is<string?>(x => x == null),
            Arg.Is<string?>(x => x == "Safari/17"),
            Arg.Is<string?>(x => x == "10.0.0.1"),
            Arg.Is<bool>(x => x == true),
            Arg.Is<CancellationToken>(x => x == cts.Token));
    }
}

