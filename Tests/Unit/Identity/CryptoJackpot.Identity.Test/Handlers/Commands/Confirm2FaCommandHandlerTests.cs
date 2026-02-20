using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentAssertions;
using FluentResults;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class Confirm2FaCommandHandlerTests
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;
    private readonly Confirm2FaCommandHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public Confirm2FaCommandHandlerTests()
    {
        _twoFactorSetupService = Substitute.For<ITwoFactorSetupService>();
        _sut = new Confirm2FaCommandHandler(_twoFactorSetupService);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid TOTP code → 2FA confirmed with recovery codes
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCode_Returns2FaConfirmedWithRecoveryCodes()
    {
        // Arrange
        var expected = new Confirm2FaResultDto
        {
            RecoveryCodes = new[] { "ABCD-EFGH", "IJKL-MNOP", "QRST-UVWX" }
        };

        _twoFactorSetupService
            .ConfirmSetupAsync(UserGuid, "123456", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expected));

        var command = new Confirm2FaCommand { UserGuid = UserGuid, Code = "123456" };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecoveryCodes.Should().HaveCount(3);
        result.Value.RecoveryCodeCount.Should().Be(3);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Wrong TOTP code → service returns error
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WrongCode_PropagatesError()
    {
        // Arrange
        _twoFactorSetupService
            .ConfirmSetupAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Confirm2FaResultDto>("Invalid TOTP code"));

        var command = new Confirm2FaCommand { UserGuid = UserGuid, Code = "000000" };

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
        const string code = "654321";
        using var cts = new CancellationTokenSource();

        _twoFactorSetupService
            .ConfirmSetupAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new Confirm2FaResultDto()));

        var command = new Confirm2FaCommand { UserGuid = UserGuid, Code = code };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _twoFactorSetupService.Received(1).ConfirmSetupAsync(UserGuid, code, cts.Token);
    }
}

