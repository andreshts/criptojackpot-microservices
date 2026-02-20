using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class Setup2FaCommandHandlerTests
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;
    private readonly Setup2FaCommandHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public Setup2FaCommandHandlerTests()
    {
        _twoFactorSetupService = Substitute.For<ITwoFactorSetupService>();
        _sut = new Setup2FaCommandHandler(_twoFactorSetupService);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Service returns success → secret and QR code returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ServiceSucceeds_ReturnsSetupResult()
    {
        // Arrange
        var expected = new Setup2FaResultDto
        {
            Secret = "BASE32SECRET",
            QrCodeUri = "otpauth://totp/CryptoJackpot:user%40test.com?secret=BASE32SECRET"
        };

        _twoFactorSetupService
            .InitiateSetupAsync(UserGuid, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expected));

        var command = new Setup2FaCommand { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Secret.Should().Be("BASE32SECRET");
        result.Value.QrCodeUri.Should().Contain("otpauth://totp/");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Service returns failure → error propagated
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ServiceFails_PropagatesError()
    {
        // Arrange
        _twoFactorSetupService
            .InitiateSetupAsync(UserGuid, Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Setup2FaResultDto>("User not found"));

        var command = new Setup2FaCommand { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Delegates correct UserGuid and CancellationToken
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_DelegatesCorrectArgumentsToService()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        _twoFactorSetupService
            .InitiateSetupAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new Setup2FaResultDto { Secret = "S", QrCodeUri = "Q" }));

        var command = new Setup2FaCommand { UserGuid = UserGuid };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _twoFactorSetupService.Received(1).InitiateSetupAsync(UserGuid, cts.Token);
    }
}

