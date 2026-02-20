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

public class ResetPasswordWithCodeCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ResetPasswordWithCodeCommandHandler _sut;

    public ResetPasswordWithCodeCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<ResetPasswordWithCodeCommandHandler>>();

        _sut = new ResetPasswordWithCodeCommandHandler(
            _userRepository,
            _passwordHasher,
            _refreshTokenRepository,
            _unitOfWork,
            logger);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static User CreateUserWithValidToken(string resetTokenHash = "hashed_code")
    {
        return new User
        {
            Id = 1,
            Email = "user@cryptojackpot.com",
            Name = "John",
            LastName = "Doe",
            PasswordHash = "old_hashed_password",
            PasswordResetToken = resetTokenHash,
            PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
    }

    private static ResetPasswordWithCodeCommand CreateCommand(
        string email = "user@cryptojackpot.com",
        string securityCode = "123456",
        string newPassword = "NewPass123!")
    {
        return new ResetPasswordWithCodeCommand
        {
            Email = email,
            SecurityCode = securityCode,
            Password = newPassword,
            ConfirmPassword = newPassword
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → BadRequestError (anti-enumeration)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsBadRequestError()
    {
        // Arrange
        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns((User?)null);
        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<BadRequestError>()
            .Which.Message.Should().Contain("Invalid or expired security code");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Token expired → BadRequestError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsBadRequestError()
    {
        // Arrange
        var user = CreateUserWithValidToken();
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1); // expired

        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<BadRequestError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Wrong security code → BadRequestError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WrongSecurityCode_ReturnsBadRequestError()
    {
        // Arrange
        const string resetToken = "hashed_token";
        var user = CreateUserWithValidToken(resetToken);

        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        // Verify returns false → wrong code
        _passwordHasher.Verify(resetToken, Arg.Any<string>()).Returns(false);

        var command = CreateCommand(securityCode: "999999");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<BadRequestError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: New password same as current → BadRequestError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NewPasswordSameAsCurrent_ReturnsBadRequestError()
    {
        // Arrange
        const string resetTokenHash = "hashed_reset_token";
        const string currentPasswordHash = "old_hashed_password";
        const string securityCode = "123456";
        const string newPassword = "SamePass123!";

        var user = CreateUserWithValidToken(resetTokenHash);
        user.PasswordHash = currentPasswordHash;

        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        // Security code is valid
        _passwordHasher.Verify(resetTokenHash, securityCode).Returns(true);
        // New password same as current
        _passwordHasher.Verify(currentPasswordHash, newPassword).Returns(true);

        var command = CreateCommand(securityCode: securityCode, newPassword: newPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<BadRequestError>()
            .Which.Message.Should().Contain("different from current password");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: Valid code, new password → resets password, revokes tokens
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCode_ResetsPasswordAndRevokesAllTokens()
    {
        // Arrange
        const string resetTokenHash = "hashed_reset_token";
        const string securityCode = "123456";
        const string newPassword = "NewSecurePass456!";
        const string newHash = "new_bcrypt_hash";

        var user = CreateUserWithValidToken(resetTokenHash);

        _userRepository.GetByEmailCaseInsensitiveAsync(Arg.Any<string>()).Returns(user);
        _passwordHasher.Verify(resetTokenHash, securityCode).Returns(true);
        _passwordHasher.Verify(user.PasswordHash!, newPassword).Returns(false); // different password
        _passwordHasher.Hash(newPassword).Returns(newHash);

        var command = CreateCommand(securityCode: securityCode, newPassword: newPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verify state mutations
        user.PasswordHash.Should().Be(newHash);
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEndAt.Should().BeNull();

        // Verify: all tokens revoked and state saved
        await _refreshTokenRepository.Received(1).RevokeAllByUserIdAsync(user.Id, "Password reset");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

