using System.Text.Json;
using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Extensions;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Application.Responses;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving all supported cryptocurrencies from the CoinPayments API v2.
/// Results are cached in Redis (or in-memory when Redis is unavailable) for 6 hours,
/// since the currency list is essentially static data.
/// </summary>
public class GetCoinPaymentCurrenciesQueryHandler
    : IRequestHandler<GetCoinPaymentCurrenciesQuery, Result<List<CoinPaymentCurrencyResponse>>>
{
    private const string CacheKey = "coinpayments:currencies";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetCoinPaymentCurrenciesQueryHandler> _logger;

    public GetCoinPaymentCurrenciesQueryHandler(
        ICoinPaymentProvider coinPaymentProvider,
        IMapper mapper,
        IDistributedCache cache,
        ILogger<GetCoinPaymentCurrenciesQueryHandler> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<List<CoinPaymentCurrencyResponse>>> Handle(
        GetCoinPaymentCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // ── 1. Cache hit ──────────────────────────────────────────────
            var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for CoinPayments currencies");
                var cachedResult = JsonSerializer.Deserialize<List<CoinPaymentCurrencyResponse>>(cached);
                return Result.Ok(cachedResult!);
            }

            // ── 2. Cache miss → call external API ─────────────────────────
            _logger.LogInformation("Cache miss — fetching supported cryptocurrencies from CoinPayments API v2");

            var (isSuccess, error, currencies) = await _coinPaymentProvider.GetCurrenciesTypedAsync(cancellationToken);

            if (!isSuccess)
            {
                _logger.LogError("CoinPayments API error while fetching currencies: {Error}", error);
                return Result.Fail(new ExternalServiceError("CoinPayments", error));
            }

            _logger.LogInformation("Successfully retrieved {Count} currencies from CoinPayments", currencies.Count);

            var result = _mapper.Map<List<CoinPaymentCurrencyResponse>>(currencies);

            // ── 3. Store in cache ─────────────────────────────────────────
            var serialized = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(
                CacheKey,
                serialized,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration },
                cancellationToken);

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
