using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetAllCountriesQueryHandler : IRequestHandler<GetAllCountriesQuery, Result<IEnumerable<CountryDto>>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IMapper _mapper;

    public GetAllCountriesQueryHandler(
        ICountryRepository countryRepository,
        IMapper mapper)
    {
        _countryRepository = countryRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CountryDto>>> Handle(
        GetAllCountriesQuery request, 
        CancellationToken cancellationToken)
    {
        var countries = await _countryRepository.GetAllCountries();
        var countryDtos = _mapper.Map<IEnumerable<CountryDto>>(countries);

        return Result.Ok(countryDtos);
    }
}

