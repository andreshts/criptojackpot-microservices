using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;
namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class LoginCommandHandlerTests
{
     // ─── Mocks (Independent: cada test recibe instancias frescas) ───
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _authService = Substitute.For<IAuthenticationService>();
        _eventPublisher = Substitute.For<IIdentityEventPublisher>();

        _sut = new LoginCommandHandler(
            _userRepository,
            _unitOfWork,
            _authService,
            _eventPublisher);
    }

    // ─── Helper: Genera un User válido con estado controlado ────────
    // Repeatable: datos fijos, sin dependencias externas.
    private static User CreateUser(
        bool emailVerified = true,
        bool twoFactorEnabled = false,
        string? passwordHash = "hashed_password",
        int failedLoginAttempts = 0,
        DateTime? lockoutEndAt = null)
    {
        return new User
        {
            Id = 1,
            UserGuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "andres@cryptojackpot.com",
            Name = "Andrés",
            LastName = "Dev",
            PasswordHash = passwordHash,
            EmailVerified = emailVerified,
            TwoFactorEnabled = twoFactorEnabled,
            FailedLoginAttempts = failedLoginAttempts,
            LockoutEndAt = lockoutEndAt,
            Status = true
        };
    }

    private static LoginCommand CreateCommand(
        string email = "andres@cryptojackpot.com",
        string password = "SecurePass123!")
    {
        return new LoginCommand
        {
            Email = email,
            Password = password,
            RememberMe = false,
            IpAddress = "127.0.0.1",
            UserAgent = "xUnit/TestRunner"
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → UnauthorizedError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnauthorizedError()
    {
        // Arrange
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        var command = CreateCommand(email: "nonexistent@test.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert (Self-validating)
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Invalid email or password");

        // Verify: no debe tocar UnitOfWork ni AuthService
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _authService.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Account locked out → LockedError with RetryAfterSeconds
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_AccountLockedOut_ReturnsLockedError()
    {
        // Arrange — Repeatable: fecha fija muy lejana, siempre en el futuro
        var user = CreateUser(lockoutEndAt: new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Utc));
        _userRepository.GetByEmailAsync(user.Email).Returns(user);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<LockedError>()
            .Which.RetryAfterSeconds.Should().BeGreaterThan(0);

        // Verify: no debe intentar verificar password
        _authService.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Google-only account (PasswordHash is null) → UnauthorizedError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_GoogleOnlyAccount_ReturnsUnauthorizedWithGoogleMessage()
    {
        // Arrange
        var user = CreateUser(passwordHash: null);
        _userRepository.GetByEmailAsync(user.Email).Returns(user);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Contain("Google sign-in");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Wrong password (below lockout threshold) → UnauthorizedError
    //           + persists failed attempt via UnitOfWork
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WrongPassword_NoLockout_ReturnsUnauthorizedAndPersistsFailedAttempt()
    {
        // Arrange — failedLoginAttempts = 0 → after RegisterFailedLogin() = 1, no lockout
        var user = CreateUser(failedLoginAttempts: 0);
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(user.PasswordHash!, Arg.Any<string>()).Returns(false);

        var command = CreateCommand(password: "WrongPass!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>();

        // Verify: debe persistir el intento fallido
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verify: no publica evento de lockout porque no se alcanzó el threshold
        await _eventPublisher.DidNotReceive()
            .PublishUserLockedOutAsync(
                Arg.Any<User>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: Wrong password triggers lockout → LockedError + event published
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WrongPassword_TriggersLockout_ReturnsLockedErrorAndPublishesEvent()
    {
        // Arrange — failedLoginAttempts = 2, after RegisterFailedLogin() = 3 → lockout 1 min
        var user = CreateUser(failedLoginAttempts: 2);
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(user.PasswordHash!, Arg.Any<string>()).Returns(false);
        _authService.GetLockoutMinutes(Arg.Any<int>()).Returns(1);

        var command = CreateCommand(password: "WrongPass!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert — RegisterFailedLogin() en el User real seteará IsLockedOut = true
        // porque failedLoginAttempts pasa de 2 → 3, activando lockout de 1 minuto.
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<LockedError>();

        // Verify: persiste el estado Y publica evento de seguridad
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1)
            .PublishUserLockedOutAsync(
                user,
                1,
                command.IpAddress,
                command.UserAgent);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 6: Email not verified → ForbiddenError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_EmailNotVerified_ReturnsForbiddenError()
    {
        // Arrange
        var user = CreateUser(emailVerified: false);
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(user.PasswordHash!, Arg.Any<string>()).Returns(true);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ForbiddenError>()
            .Which.Message.Should().Contain("verify your email");

        // Verify: no debe completar login ni generar tokens
        await _authService.DidNotReceive()
            .CompleteLoginAsync(
                Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 7: 2FA enabled → Delegates to HandleTwoFactorLoginAsync
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_TwoFactorEnabled_ReturnsTwoFactorChallengeResult()
    {
        // Arrange
        var user = CreateUser(twoFactorEnabled: true);
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(user.PasswordHash!, Arg.Any<string>()).Returns(true);

        var twoFactorResult = new LoginResultDto
        {
            AccessToken = "challenge_token_jwt",
            RefreshToken = string.Empty,
            ExpiresInMinutes = 5,
            RequiresTwoFactor = true,
            User = new AuthResponseDto
            {
                UserGuid = user.UserGuid,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                RequiresTwoFactor = true
            }
        };

        _authService.HandleTwoFactorLoginAsync(user, Arg.Any<CancellationToken>())
            .Returns(twoFactorResult);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresTwoFactor.Should().BeTrue();
        result.Value.RefreshToken.Should().BeEmpty("no refresh token until 2FA is verified");
        result.Value.ExpiresInMinutes.Should().Be(5);

        // Verify: no debe llamar CompleteLoginAsync
        await _authService.DidNotReceive()
            .CompleteLoginAsync(
                Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>());

        // Verify: sí delegó al flujo 2FA
        await _authService.Received(1)
            .HandleTwoFactorLoginAsync(user, Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 8: Successful login → LoginResultDto with tokens
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var user = CreateUser();
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(user.PasswordHash!, Arg.Any<string>()).Returns(true);

        var expectedResult = new LoginResultDto
        {
            AccessToken = "jwt_access_token",
            RefreshToken = "secure_refresh_token",
            ExpiresInMinutes = 15,
            RequiresTwoFactor = false,
            User = new AuthResponseDto
            {
                UserGuid = user.UserGuid,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                EmailVerified = true,
                TwoFactorEnabled = false,
                ExpiresIn = 900
            }
        };

        _authService.CompleteLoginAsync(
                user,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert (Self-validating)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Value.RequiresTwoFactor.Should().BeFalse();
        result.Value.User.Email.Should().Be(user.Email);

        // Verify: completó el flujo de login estándar
        await _authService.Received(1).CompleteLoginAsync(
            user,
            command.UserAgent,
            command.IpAddress,
            command.RememberMe,
            Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Parametric: Password verification receives correct arguments
    // ═════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("my_password_123")]
    [InlineData("P@ssw0rd!Complex")]
    [InlineData("simple")]
    public async Task Handle_PasswordVerification_PassesCorrectHashAndPassword(string password)
    {
        // Arrange
        var user = CreateUser();
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var command = CreateCommand(password: password);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — verifica que el hash almacenado y el password del request se pasan tal cual
        _authService.Received(1).VerifyPassword(user.PasswordHash!, password);
    }

    // ═════════════════════════════════════════════════════════════════
    // Edge case: CancellationToken propagation
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_CancellationToken_PropagatedToAuthService()
    {
        // Arrange
        var user = CreateUser();
        _userRepository.GetByEmailAsync(user.Email).Returns(user);
        _authService.VerifyPassword(user.PasswordHash!, Arg.Any<string>()).Returns(true);

        var expectedResult = new LoginResultDto
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            ExpiresInMinutes = 15,
            RequiresTwoFactor = false,
            User = new AuthResponseDto { Email = user.Email, Name = user.Name, LastName = user.LastName }
        };

        _authService.CompleteLoginAsync(
                Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        using var cts = new CancellationTokenSource();
        var command = CreateCommand();

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert — verifica que el CancellationToken se propaga correctamente
        await _authService.Received(1).CompleteLoginAsync(
            Arg.Any<User>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            cts.Token);
    }
}