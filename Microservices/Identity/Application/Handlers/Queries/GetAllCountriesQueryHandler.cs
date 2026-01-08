using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetAllCountriesQueryHandler : IRequestHandler<GetAllCountriesQuery, Result<IEnumerable<CountryDto>>>
{
    private readonly ICountryRepository _countryRepository;

    public GetAllCountriesQueryHandler(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<Result<IEnumerable<CountryDto>>> Handle(
        GetAllCountriesQuery request, 
        CancellationToken cancellationToken)
    {
        var countries = await _countryRepository.GetAllCountries();

        var countryDtos = countries.Select(c => new CountryDto
        {
            Id = c.Id,
            Name = c.Name,
            Iso2 = c.Iso2,
            Iso3 = c.Iso3,
            PhoneCode = c.PhoneCode,
            Currency = c.Currency,
            CurrencySymbol = c.CurrencySymbol,
            Region = c.Region
        });

        return Result.Ok(countryDtos);
    }
}

