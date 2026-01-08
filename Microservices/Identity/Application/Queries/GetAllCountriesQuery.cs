using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetAllCountriesQuery : IRequest<Result<IEnumerable<CountryDto>>>
{
}

