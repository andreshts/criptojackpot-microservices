using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class Disable2FaCommandHandlerTests
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;
    private readonly Disable2FaCommandHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public Disable2FaCommandHandlerTests()
    {
        _twoFactorSetupService = Substitute.For<ITwoFactorSetupService>();
        _sut = new Disable2FaCommandHandler(_twoFactorSetupService);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid TOTP code → 2FA disabled successfully
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidTotpCode_Disables2FaSuccessfully()
    {
        // Arrange
        _twoFactorSetupService
            .DisableAsync(UserGuid, "123456", null, Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var command = new Disable2FaCommand { UserGuid = UserGuid, Code = "123456" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Valid recovery code → 2FA disabled successfully
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidRecoveryCode_Disables2FaSuccessfully()
    {
        // Arrange
        _twoFactorSetupService
            .DisableAsync(UserGuid, null, "ABCD-EFGH", Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var command = new Disable2FaCommand { UserGuid = UserGuid, RecoveryCode = "ABCD-EFGH" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Service returns error → propagates failure
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidCode_PropagatesError()
    {
        // Arrange
        _twoFactorSetupService
            .DisableAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Invalid verification code"));

        var command = new Disable2FaCommand { UserGuid = UserGuid, Code = "000000" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Correct arguments delegated (code + recovery code)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_DelegatesAllArgumentsToService()
    {
        // Arrange
        const string code = "654321";
        const string recoveryCode = "XXXX-YYYY";
        using var cts = new CancellationTokenSource();

        _twoFactorSetupService
            .DisableAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var command = new Disable2FaCommand { UserGuid = UserGuid, Code = code, RecoveryCode = recoveryCode };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _twoFactorSetupService.Received(1).DisableAsync(UserGuid, code, recoveryCode, cts.Token);
    }
}

