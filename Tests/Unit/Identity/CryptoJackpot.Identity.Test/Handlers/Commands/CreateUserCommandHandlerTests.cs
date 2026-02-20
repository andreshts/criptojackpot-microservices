using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class CreateUserCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _eventPublisher = Substitute.For<IIdentityEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        var publisher = Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<CreateUserCommandHandler>>();
        _roleRepository = Substitute.For<IRoleRepository>();

        _sut = new CreateUserCommandHandler(
            _userRepository,
            _passwordHasher,
            _eventPublisher,
            _mapper,
            publisher,
            logger,
            _roleRepository);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static CreateUserCommand CreateCommand(
        string email = "test@cryptojackpot.com",
        string password = "SecurePass123!",
        string? referralCode = null)
    {
        return new CreateUserCommand
        {
            Name = "John",
            LastName = "Doe",
            Email = email,
            Password = password,
            CountryId = 1,
            StatePlace = "California",
            City = "Los Angeles",
            Status = true,
            ReferralCode = referralCode
        };
    }

    private static User CreateUser(long id = 1, string email = "test@cryptojackpot.com")
    {
        return new User
        {
            Id = id,
            UserGuid = Guid.NewGuid(),
            Email = email,
            Name = "John",
            LastName = "Doe",
            PasswordHash = "hashed_password",
            EmailVerified = false,
            Status = true
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Email already exists → ConflictError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsConflictError()
    {
        // Arrange
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(true);
        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ConflictError>()
            .Which.Message.Should().Contain("email already exists");

        // Verify: should not create user
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
        await _eventPublisher.DidNotReceive()
            .PublishUserRegisteredAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Invalid referral code → BadRequestError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_InvalidReferralCode_ReturnsBadRequestError()
    {
        // Arrange
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _userRepository.GetByReferralCodeAsync(Arg.Any<string>()).Returns((User?)null);

        var command = CreateCommand(referralCode: "INVALID123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<BadRequestError>()
            .Which.Message.Should().Contain("Invalid referral code");

        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Successful registration without referral code
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCommandNoReferral_ReturnsCreatedUser()
    {
        // Arrange
        var user = CreateUser();
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName };

        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _mapper.Map<User>(Arg.Any<CreateUserCommand>()).Returns(user);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(user);
        _roleRepository.GetDefaultRoleAsync().Returns((Role?)null);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(user.Email);

        // Verify: user is created and event is published
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>());
        await _eventPublisher.Received(1)
            .PublishUserRegisteredAsync(user, Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Successful registration with valid referral code
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCommandWithReferral_PublishesDomainEvent()
    {
        // Arrange
        var referrer = CreateUser(id: 99, email: "referrer@cryptojackpot.com");
        var newUser = CreateUser();
        var userDto = new UserDto { Id = 1, Email = newUser.Email, Name = newUser.Name, LastName = newUser.LastName };

        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _userRepository.GetByReferralCodeAsync("VALID123").Returns(referrer);
        _mapper.Map<User>(Arg.Any<CreateUserCommand>()).Returns(newUser);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(newUser);
        _roleRepository.GetDefaultRoleAsync().Returns((Role?)null);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);

        var command = CreateCommand(referralCode: "VALID123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify: user created and registered event published
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>());
        await _eventPublisher.Received(1)
            .PublishUserRegisteredAsync(newUser, Arg.Any<string>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: Exception during creation → InternalServerError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _mapper.Map<User>(Arg.Any<CreateUserCommand>()).Returns(CreateUser());
        _userRepository.CreateAsync(Arg.Any<User>())
            .ThrowsAsync(new Exception("Database connection lost"));

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<InternalServerError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 6: Password is hashed before saving
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidCommand_HashesPasswordBeforeSaving()
    {
        // Arrange
        var user = CreateUser();
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName };
        const string rawPassword = "SecurePass123!";
        const string hashedPassword = "bcrypt_hashed_value";

        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _mapper.Map<User>(Arg.Any<CreateUserCommand>()).Returns(user);
        _passwordHasher.Hash(rawPassword).Returns(hashedPassword);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(user);
        _roleRepository.GetDefaultRoleAsync().Returns((Role?)null);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);

        var command = CreateCommand(password: rawPassword);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert: password hasher was called with raw password
        _passwordHasher.Received(1).Hash(rawPassword);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 7: Default role assigned from repository
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_DefaultRoleExists_AssignsRoleIdToUser()
    {
        // Arrange
        var user = CreateUser();
        var defaultRole = new Role { Id = 3, Name = "Player" };
        var userDto = new UserDto { Id = 1, Email = user.Email, Name = user.Name, LastName = user.LastName };

        _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _mapper.Map<User>(Arg.Any<CreateUserCommand>()).Returns(user);
        _roleRepository.GetDefaultRoleAsync().Returns(defaultRole);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(user);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);

        var command = CreateCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.RoleId.Should().Be(defaultRole.Id);
    }
}

