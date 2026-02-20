using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Handlers.Queries;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Queries;

public class Get2FaStatusQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly Get2FaStatusQueryHandler _sut;

    private static readonly Guid UserGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public Get2FaStatusQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new Get2FaStatusQueryHandler(_userRepository);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static User CreateUser(
        bool twoFactorEnabled = false,
        string? twoFactorSecret = null,
        int totalRecoveryCodes = 0,
        int usedRecoveryCodes = 0)
    {
        var user = new User
        {
            Id = 1,
            UserGuid = UserGuid,
            Email = "user@cryptojackpot.com",
            Name = "John",
            LastName = "Doe",
            TwoFactorEnabled = twoFactorEnabled,
            TwoFactorSecret = twoFactorSecret
        };

        for (var i = 0; i < totalRecoveryCodes; i++)
        {
            user.RecoveryCodes.Add(new UserRecoveryCode
            {
                Id = i + 1,
                UserId = user.Id,
                CodeHash = $"hash_{i}",
                IsUsed = i < usedRecoveryCodes
            });
        }

        return user;
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → NotFoundError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository.GetByGuidWithRecoveryCodesAsync(UserGuid).Returns((User?)null);
        var query = new Get2FaStatusQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: 2FA disabled, no secret → IsEnabled=false, IsPendingSetup=false
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_TwoFactorDisabledNoSecret_ReturnsDisabledNotPending()
    {
        // Arrange
        var user = CreateUser(twoFactorEnabled: false, twoFactorSecret: null);
        _userRepository.GetByGuidWithRecoveryCodesAsync(UserGuid).Returns(user);

        var query = new Get2FaStatusQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsEnabled.Should().BeFalse();
        result.Value.IsPendingSetup.Should().BeFalse();
        result.Value.RecoveryCodesRemaining.Should().BeNull();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: 2FA disabled but secret exists → IsPendingSetup=true
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_TwoFactorDisabledWithSecret_ReturnsPendingSetup()
    {
        // Arrange
        var user = CreateUser(twoFactorEnabled: false, twoFactorSecret: "BASE32SECRET");
        _userRepository.GetByGuidWithRecoveryCodesAsync(UserGuid).Returns(user);

        var query = new Get2FaStatusQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsEnabled.Should().BeFalse();
        result.Value.IsPendingSetup.Should().BeTrue();
        result.Value.RecoveryCodesRemaining.Should().BeNull();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: 2FA enabled → IsEnabled=true with recovery codes count
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_TwoFactorEnabled_ReturnsEnabledWithRecoveryCodeCount()
    {
        // Arrange — 8 total, 3 used → 5 remaining
        var user = CreateUser(
            twoFactorEnabled: true,
            twoFactorSecret: "BASE32SECRET",
            totalRecoveryCodes: 8,
            usedRecoveryCodes: 3);

        _userRepository.GetByGuidWithRecoveryCodesAsync(UserGuid).Returns(user);

        var query = new Get2FaStatusQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsEnabled.Should().BeTrue();
        result.Value.IsPendingSetup.Should().BeFalse();
        result.Value.RecoveryCodesRemaining.Should().Be(5);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: 2FA enabled, all recovery codes used → 0 remaining
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_AllRecoveryCodesUsed_ReturnsZeroRemaining()
    {
        // Arrange — 8 total, 8 used
        var user = CreateUser(
            twoFactorEnabled: true,
            twoFactorSecret: "SECRET",
            totalRecoveryCodes: 8,
            usedRecoveryCodes: 8);

        _userRepository.GetByGuidWithRecoveryCodesAsync(UserGuid).Returns(user);

        var query = new Get2FaStatusQuery { UserGuid = UserGuid };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecoveryCodesRemaining.Should().Be(0);
    }
}

