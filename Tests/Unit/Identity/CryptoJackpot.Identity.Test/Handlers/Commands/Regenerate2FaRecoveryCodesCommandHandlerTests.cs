using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class Regenerate2FaRecoveryCodesCommandHandlerTests
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;
    private readonly Regenerate2FaRecoveryCodesCommandHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public Regenerate2FaRecoveryCodesCommandHandlerTests()
    {
        _twoFactorSetupService = Substitute.For<ITwoFactorSetupService>();
        _sut = new Regenerate2FaRecoveryCodesCommandHandler(_twoFactorSetupService);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid code → new recovery codes returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCode_ReturnsNewRecoveryCodes()
    {
        // Arrange
        var expected = new Confirm2FaResultDto
        {
            RecoveryCodes = new[] { "AA11-BB22", "CC33-DD44", "EE55-FF66", "GG77-HH88" }
        };

        _twoFactorSetupService
            .RegenerateRecoveryCodesAsync(UserGuid, "123456", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expected));

        var command = new Regenerate2FaRecoveryCodesCommand { UserGuid = UserGuid, Code = "123456" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecoveryCodes.Should().HaveCount(4);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Invalid TOTP code → service error propagated
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidCode_PropagatesError()
    {
        // Arrange
        _twoFactorSetupService
            .RegenerateRecoveryCodesAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Confirm2FaResultDto>("Invalid TOTP code"));

        var command = new Regenerate2FaRecoveryCodesCommand { UserGuid = UserGuid, Code = "000000" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Correct arguments delegated
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_DelegatesCorrectArgumentsToService()
    {
        // Arrange
        const string code = "987654";
        using var cts = new CancellationTokenSource();

        _twoFactorSetupService
            .RegenerateRecoveryCodesAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new Confirm2FaResultDto()));

        var command = new Regenerate2FaRecoveryCodesCommand { UserGuid = UserGuid, Code = code };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _twoFactorSetupService.Received(1).RegenerateRecoveryCodesAsync(UserGuid, code, cts.Token);
    }
}

