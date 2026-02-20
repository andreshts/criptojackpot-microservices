using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class UpdateUserCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();

        _sut = new UpdateUserCommandHandler(_userRepository, _mapper);
    }

    // ─── Helpers ────────────────────────────────────────────────────
    private static UpdateUserCommand CreateCommand(long userId = 1)
    {
        return new UpdateUserCommand
        {
            UserId = userId,
            Name = "UpdatedName",
            LastName = "UpdatedLastName",
            Phone = "+1234567890",
            CountryId = 2,
            StatePlace = "New York",
            City = "New York City",
            Address = "123 Main St"
        };
    }

    private static User CreateUser(long id = 1)
    {
        return new User
        {
            Id = id,
            UserGuid = Guid.NewGuid(),
            Email = "user@cryptojackpot.com",
            Name = "OldName",
            LastName = "OldLastName",
            Status = true
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: User not found → NotFoundError
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<long>()).Returns((User?)null);
        var command = CreateCommand(userId: 999);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Be("User not found");

        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Successful update → mapped UserDto returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserExists_UpdatesFieldsAndReturnsDto()
    {
        // Arrange
        var user = CreateUser();
        var command = CreateCommand();
        var updatedDto = new UserDto
        {
            Id = 1,
            Name = command.Name,
            LastName = command.LastName,
            Phone = command.Phone,
            City = command.City,
            StatePlace = command.StatePlace,
            Address = command.Address
        };

        _userRepository.GetByIdAsync(1).Returns(user);
        _userRepository.UpdateAsync(Arg.Any<User>()).Returns(user);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(updatedDto);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(command.Name);
        result.Value.LastName.Should().Be(command.LastName);
        result.Value.Phone.Should().Be(command.Phone);

        // Verify: repository updated
        await _userRepository.Received(1).UpdateAsync(user);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: User fields are mutated before update call
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_UserExists_MutatesUserPropertiesBeforeUpdate()
    {
        // Arrange
        var user = CreateUser();
        var command = CreateCommand();
        var updatedDto = new UserDto { Id = 1, Name = command.Name, LastName = command.LastName };

        _userRepository.GetByIdAsync(1).Returns(user);
        _userRepository.UpdateAsync(Arg.Any<User>()).Returns(user);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(updatedDto);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert: user entity fields were mutated with command data
        user.Name.Should().Be(command.Name);
        user.LastName.Should().Be(command.LastName);
        user.Phone.Should().Be(command.Phone);
        user.CountryId.Should().Be(command.CountryId);
        user.StatePlace.Should().Be(command.StatePlace);
        user.City.Should().Be(command.City);
        user.Address.Should().Be(command.Address);
    }
}

