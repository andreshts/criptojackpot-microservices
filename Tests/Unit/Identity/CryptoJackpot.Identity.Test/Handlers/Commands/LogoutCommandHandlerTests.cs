using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class LogoutCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IAuthenticationService _authService;
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _authService = Substitute.For<IAuthenticationService>();
        _sut = new LogoutCommandHandler(_authService);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid refresh token → revoked and returns Ok
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithValidRefreshToken_RevokesTokenAndReturnsOk()
    {
        // Arrange
        const string refreshToken = "valid_refresh_token_abc";
        var command = new LogoutCommand { RefreshToken = refreshToken };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.Received(1).RevokeRefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Null refresh token → skips revocation but returns Ok
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithNullRefreshToken_DoesNotRevokeAndReturnsOk()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = null };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.DidNotReceive().RevokeRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Empty refresh token → skips revocation but returns Ok
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_DoesNotRevokeAndReturnsOk()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = string.Empty };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.DidNotReceive().RevokeRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Whitespace refresh token → skips revocation
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithWhitespaceRefreshToken_DoesNotRevokeAndReturnsOk()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = "   " };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.DidNotReceive().RevokeRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: CancellationToken propagated to revocation call
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        const string token = "some_refresh_token";
        var command = new LogoutCommand { RefreshToken = token };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _authService.Received(1).RevokeRefreshTokenAsync(token, cts.Token);
    }
}

