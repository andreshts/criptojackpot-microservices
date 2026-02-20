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

public class GetReferralStatsQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IUserReferralRepository _userReferralRepository;
    private readonly IMapper _mapper;
    private readonly GetReferralStatsQueryHandler _sut;

    public GetReferralStatsQueryHandlerTests()
    {
        _userReferralRepository = Substitute.For<IUserReferralRepository>();
        _mapper = Substitute.For<IMapper>();
        _sut = new GetReferralStatsQueryHandler(_userReferralRepository, _mapper);
    }

    // ─── Helper ─────────────────────────────────────────────────────
    private static UserReferralWithStats CreateReferralStats(string email = "referred@test.com") => new()
    {
        Email = email,
        FullName = "John Doe",
        UsedSecurityCode = "REF001",
        RegisterDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc)
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: No referrals → empty list with zero earnings
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NoReferrals_ReturnsEmptyStatsWithZeroEarnings()
    {
        // Arrange
        _userReferralRepository.GetReferralStatsAsync(Arg.Any<long>())
            .Returns(Enumerable.Empty<UserReferralWithStats>());
        _mapper.Map<IEnumerable<UserReferralDto>>(Arg.Any<IEnumerable<UserReferralWithStats>>())
            .Returns(Enumerable.Empty<UserReferralDto>());

        var query = new GetReferralStatsQuery { UserId = 1 };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Referrals.Should().BeEmpty();
        result.Value.TotalEarnings.Should().Be(0);
        result.Value.LastMonthEarnings.Should().Be(0);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Referrals exist → mapped DTOs returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ReferralsExist_ReturnsMappedReferralDtos()
    {
        // Arrange
        var referralStats = new[]
        {
            CreateReferralStats("user1@test.com"),
            CreateReferralStats("user2@test.com"),
            CreateReferralStats("user3@test.com")
        };

        var dtos = referralStats.Select(r => new UserReferralDto
        {
            Email = r.Email,
            FullName = r.FullName,
            UsedSecurityCode = r.UsedSecurityCode,
            RegisterDate = r.RegisterDate
        }).ToList();

        _userReferralRepository.GetReferralStatsAsync(42).Returns(referralStats);
        _mapper.Map<IEnumerable<UserReferralDto>>(Arg.Any<IEnumerable<UserReferralWithStats>>()).Returns(dtos);

        var query = new GetReferralStatsQuery { UserId = 42 };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Referrals.Should().HaveCount(3);
        result.Value.Referrals.Select(r => r.Email).Should().Contain("user1@test.com", "user2@test.com", "user3@test.com");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Correct UserId is queried
    // ═════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task Handle_CallsRepositoryWithCorrectUserId(long userId)
    {
        // Arrange
        _userReferralRepository.GetReferralStatsAsync(Arg.Any<long>())
            .Returns(Enumerable.Empty<UserReferralWithStats>());
        _mapper.Map<IEnumerable<UserReferralDto>>(Arg.Any<IEnumerable<UserReferralWithStats>>())
            .Returns(Enumerable.Empty<UserReferralDto>());

        var query = new GetReferralStatsQuery { UserId = userId };

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _userReferralRepository.Received(1).GetReferralStatsAsync(userId);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Earnings always zero (pending business logic)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithReferrals_EarningsAlwaysZero()
    {
        // Arrange — earnings calculation is not yet implemented
        var stats = new[] { CreateReferralStats() };
        var dtos = new[] { new UserReferralDto { Email = "r@test.com", FullName = "Ref", UsedSecurityCode = "C" } };

        _userReferralRepository.GetReferralStatsAsync(Arg.Any<long>()).Returns(stats);
        _mapper.Map<IEnumerable<UserReferralDto>>(Arg.Any<IEnumerable<UserReferralWithStats>>()).Returns(dtos);

        var query = new GetReferralStatsQuery { UserId = 1 };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert — handler hardcodes 0 until earnings logic is added
        result.Value.TotalEarnings.Should().Be(0, "earnings calculation not yet implemented");
        result.Value.LastMonthEarnings.Should().Be(0, "earnings calculation not yet implemented");
    }
}

