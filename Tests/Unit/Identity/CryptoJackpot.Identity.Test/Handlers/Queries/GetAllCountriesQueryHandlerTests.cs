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

public class GetAllCountriesQueryHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly ICountryRepository _countryRepository;
    private readonly IMapper _mapper;
    private readonly GetAllCountriesQueryHandler _sut;

    public GetAllCountriesQueryHandlerTests()
    {
        _countryRepository = Substitute.For<ICountryRepository>();
        _mapper = Substitute.For<IMapper>();
        _sut = new GetAllCountriesQueryHandler(_countryRepository, _mapper);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: No countries in repository → empty list
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NoCountries_ReturnsEmptyList()
    {
        // Arrange
        _countryRepository.GetAllCountries().Returns(new List<Country>());
        _mapper.Map<IEnumerable<CountryDto>>(Arg.Any<IEnumerable<Country>>())
            .Returns(Enumerable.Empty<CountryDto>());

        var query = new GetAllCountriesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Countries exist → mapped DTOs returned
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_CountriesExist_ReturnsMappedDtos()
    {
        // Arrange
        var countries = new List<Country>
        {
            new() { Id = 1, Name = "Colombia", Iso2 = "CO", PhoneCode = "+57" },
            new() { Id = 2, Name = "United States", Iso2 = "US", PhoneCode = "+1" },
            new() { Id = 3, Name = "Spain", Iso2 = "ES", PhoneCode = "+34" }
        };

        var dtos = countries.Select(c => new CountryDto { Id = c.Id, Name = c.Name, Iso2 = c.Iso2, PhoneCode = c.PhoneCode });

        _countryRepository.GetAllCountries().Returns(countries);
        _mapper.Map<IEnumerable<CountryDto>>(Arg.Any<IEnumerable<Country>>()).Returns(dtos);

        var query = new GetAllCountriesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Select(c => c.Name).Should().Contain("Colombia", "United States", "Spain");
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Repository is called exactly once
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_AlwaysCallsRepositoryOnce()
    {
        // Arrange
        _countryRepository.GetAllCountries().Returns(new List<Country>());
        _mapper.Map<IEnumerable<CountryDto>>(Arg.Any<IEnumerable<Country>>())
            .Returns(Enumerable.Empty<CountryDto>());

        var query = new GetAllCountriesQuery();

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _countryRepository.Received(1).GetAllCountries();
    }
}

