using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Extensions;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Application.Responses;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving all supported cryptocurrencies from the CoinPayments API v2.
/// </summary>
public class GetCoinPaymentCurrenciesQueryHandler
    : IRequestHandler<GetCoinPaymentCurrenciesQuery, Result<List<CoinPaymentCurrencyResponse>>>
{
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCoinPaymentCurrenciesQueryHandler> _logger;

    public GetCoinPaymentCurrenciesQueryHandler(
        ICoinPaymentProvider coinPaymentProvider,
        IMapper mapper,
        ILogger<GetCoinPaymentCurrenciesQueryHandler> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<CoinPaymentCurrencyResponse>>> Handle(
        GetCoinPaymentCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching supported cryptocurrencies from CoinPayments API v2");

            var (isSuccess, error, currencies) = await _coinPaymentProvider.GetCurrenciesTypedAsync(cancellationToken);

            if (!isSuccess)
            {
                _logger.LogError("CoinPayments API error while fetching currencies: {Error}", error);
                return Result.Fail(new ExternalServiceError("CoinPayments", error));
            }

            _logger.LogInformation("Successfully retrieved {Count} currencies from CoinPayments", currencies.Count);

            var result = _mapper.Map<List<CoinPaymentCurrencyResponse>>(currencies);
            return Result.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching CoinPayments currencies");
            return Result.Fail(new ExternalServiceError("CoinPayments", $"Unexpected error: {ex.Message}"));
        }
    }
}
