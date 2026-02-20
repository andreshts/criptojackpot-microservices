using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Handlers.Queries;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Queries;

public class GetAllRolesQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;
    private readonly GetAllRolesQueryHandler _sut;

    public GetAllRolesQueryHandlerTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _mapper = Substitute.For<IMapper>();
        _sut = new GetAllRolesQueryHandler(_roleRepository, _mapper);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: No roles in repository → empty list
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NoRoles_ReturnsEmptyList()
    {
        // Arrange
        _roleRepository.GetAllRoles().Returns(new List<Role>());
        _mapper.Map<IEnumerable<RoleDto>>(Arg.Any<IEnumerable<Role>>())
            .Returns(Enumerable.Empty<RoleDto>());

        var query = new GetAllRolesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Roles exist → mapped DTOs returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_RolesExist_ReturnsMappedDtos()
    {
        // Arrange
        var roles = new List<Role>
        {
            new() { Id = 1, Name = "admin", Description = "Administrator" },
            new() { Id = 2, Name = "client", Description = "Regular user" }
        };

        var dtos = roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name, Description = r.Description });

        _roleRepository.GetAllRoles().Returns(roles);
        _mapper.Map<IEnumerable<RoleDto>>(Arg.Any<IEnumerable<Role>>()).Returns(dtos);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(r => r.Name).Should().Contain("admin", "client");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Repository is called exactly once
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_AlwaysCallsRepositoryOnce()
    {
        // Arrange
        _roleRepository.GetAllRoles().Returns(new List<Role>());
        _mapper.Map<IEnumerable<RoleDto>>(Arg.Any<IEnumerable<Role>>())
            .Returns(Enumerable.Empty<RoleDto>());

        var query = new GetAllRolesQuery();

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _roleRepository.Received(1).GetAllRoles();
    }
}

