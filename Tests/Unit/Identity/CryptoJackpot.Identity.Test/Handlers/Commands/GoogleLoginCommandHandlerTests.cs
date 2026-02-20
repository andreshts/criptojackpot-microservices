using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class GoogleLoginCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IGoogleLoginService _googleLoginService;
    private readonly GoogleLoginCommandHandler _sut;

    public GoogleLoginCommandHandlerTests()
    {
        _googleAuthService = Substitute.For<IGoogleAuthService>();
        _googleLoginService = Substitute.For<IGoogleLoginService>();
        _sut = new GoogleLoginCommandHandler(_googleAuthService, _googleLoginService);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static GoogleLoginCommand CreateCommand(string idToken = "valid_google_id_token")
    {
        return new GoogleLoginCommand
        {
            IdToken = idToken,
            AccessToken = "google_access_token",
            RefreshToken = "google_refresh_token",
            ExpiresIn = 3600,
            IpAddress = "127.0.0.1",
            DeviceInfo = "Chrome/120",
            RememberMe = false
        };
    }

    private static GoogleUserPayload CreatePayload() => new()
    {
        GoogleId = "google-sub-12345",
        Email = "user@gmail.com",
        EmailVerified = true,
        GivenName = "John",
        FamilyName = "Doe",
        Name = "John Doe"
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Invalid Google ID token → UnauthorizedError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidIdToken_ReturnsUnauthorizedError()
    {
        // Arrange
        _googleAuthService.ValidateIdTokenAsync(Arg.Any<string>()).Returns((GoogleUserPayload?)null);
        var command = CreateCommand(idToken: "invalid_token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Contain("Invalid Google ID token");

        // Verify: login service never called
        await _googleLoginService.DidNotReceive()
            .LoginOrRegisterAsync(Arg.Any<GoogleUserPayload>(), Arg.Any<GoogleLoginContext>(), Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Valid token, successful login/register
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidToken_ReturnsLoginResult()
    {
        // Arrange
        var payload = CreatePayload();
        var loginResult = new LoginResultDto
        {
            AccessToken = "jwt_token",
            RefreshToken = "refresh_token",
            ExpiresInMinutes = 15,
            RequiresTwoFactor = false,
            User = new AuthResponseDto { Email = payload.Email, Name = "John", LastName = "Doe" }
        };

        _googleAuthService.ValidateIdTokenAsync(Arg.Any<string>()).Returns(payload);
        _googleLoginService
            .LoginOrRegisterAsync(Arg.Any<GoogleUserPayload>(), Arg.Any<GoogleLoginContext>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(loginResult));

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt_token");
        result.Value.RequiresTwoFactor.Should().BeFalse();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Valid token but login service returns error
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_LoginServiceFails_PropagatesError()
    {
        // Arrange
        var payload = CreatePayload();

        _googleAuthService.ValidateIdTokenAsync(Arg.Any<string>()).Returns(payload);
        _googleLoginService
            .LoginOrRegisterAsync(Arg.Any<GoogleUserPayload>(), Arg.Any<GoogleLoginContext>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<LoginResultDto>("Registration failed due to internal error"));

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Context is built from command properties
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidToken_BuildsContextFromCommand()
    {
        // Arrange
        var payload = CreatePayload();
        var loginResult = new LoginResultDto
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            ExpiresInMinutes = 15,
            User = new AuthResponseDto { Email = "user@gmail.com", Name = "John", LastName = "Doe" }
        };

        _googleAuthService.ValidateIdTokenAsync(Arg.Any<string>()).Returns(payload);
        _googleLoginService
            .LoginOrRegisterAsync(Arg.Any<GoogleUserPayload>(), Arg.Any<GoogleLoginContext>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(loginResult));

        var command = CreateCommand();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert: verify context was passed with correct values
        await _googleLoginService.Received(1).LoginOrRegisterAsync(
            payload,
            Arg.Is<GoogleLoginContext>(ctx =>
                ctx.AccessToken == command.AccessToken &&
                ctx.IpAddress == command.IpAddress &&
                ctx.DeviceInfo == command.DeviceInfo &&
                ctx.RememberMe == command.RememberMe),
            Arg.Any<CancellationToken>());
    }
}

