using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class RefreshTokenCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly ITokenRotationService _tokenRotationService;
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _tokenRotationService = Substitute.For<ITokenRotationService>();
        _sut = new RefreshTokenCommandHandler(_tokenRotationService);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static RefreshTokenCommand CreateCommand(string token = "valid_refresh_token")
    {
        return new RefreshTokenCommand
        {
            RefreshToken = token,
            DeviceInfo = "Mozilla/5.0",
            IpAddress = "127.0.0.1"
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid token → successful rotation result
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokenPair()
    {
        // Arrange
        var expected = new TokenRotationResultDto
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            ExpiresInMinutes = 15,
            IsRememberMe = false
        };

        _tokenRotationService
            .RotateTokenAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expected));

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be("new_refresh_token");
        result.Value.ExpiresInMinutes.Should().Be(15);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Invalid/reused token → failure from rotation service
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidToken_ReturnsFailed()
    {
        // Arrange
        _tokenRotationService
            .RotateTokenAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<TokenRotationResultDto>("Invalid or expired refresh token"));

        var command = CreateCommand(token: "stolen_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Correct arguments delegated to rotation service
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_DelegatesCorrectArgumentsToRotationService()
    {
        // Arrange
        const string rawToken = "raw_refresh_token_value";
        const string deviceInfo = "Chrome/120";
        const string ipAddress = "192.168.1.1";

        _tokenRotationService
            .RotateTokenAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new TokenRotationResultDto
            {
                AccessToken = "access",
                RefreshToken = "refresh",
                ExpiresInMinutes = 15
            }));

        var command = new RefreshTokenCommand
        {
            RefreshToken = rawToken,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert: correct args passed to service
        await _tokenRotationService.Received(1).RotateTokenAsync(
            rawToken,
            deviceInfo,
            ipAddress,
            Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: CancellationToken propagated
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        _tokenRotationService
            .RotateTokenAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new TokenRotationResultDto
            {
                AccessToken = "a",
                RefreshToken = "r",
                ExpiresInMinutes = 15
            }));

        var command = CreateCommand();

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _tokenRotationService.Received(1).RotateTokenAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            cts.Token);
    }
}

