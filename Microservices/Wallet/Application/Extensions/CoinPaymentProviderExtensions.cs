using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Domain.Interfaces;

namespace CryptoJackpot.Wallet.Application.Extensions;

/// <summary>
/// Typed wrappers over ICoinPaymentProvider for the new CoinPayments API v2.
/// </summary>
public static class CoinPaymentProviderExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // ── Invoices ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new payment invoice. Maps from the legacy CreateTransactionRequest for backward compat.
    /// </summary>
    public static async Task<CoinPaymentsApiResponse<CreateTransactionResult>?> CreateTransactionAsync(
        this ICoinPaymentProvider provider,
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await provider.CreateInvoiceAsync(
            amount:           request.Amount,
            currencyFrom:     request.CurrencyFrom,
            currencyTo:       request.CurrencyTo,
            notes:            request.ItemName,
            notificationsUrl: request.IpnUrl,
            cancellationToken: cancellationToken);

        return Deserialize<CoinPaymentsApiResponse<CreateTransactionResult>>(response);
    }

    /// <summary>Gets information about a specific invoice by ID.</summary>
    public static async Task<CoinPaymentsApiResponse<TransactionInfoResult>?> GetTransactionInfoAsync(
        this ICoinPaymentProvider provider,
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var response = await provider.GetInvoiceAsync(transactionId, cancellationToken);
        return Deserialize<CoinPaymentsApiResponse<TransactionInfoResult>>(response);
    }

    // ── Balances ──────────────────────────────────────────────────────────

    /// <summary>Gets merchant wallet balances.</summary>
    public static async Task<CoinPaymentsApiResponse<List<BalanceResult>>?> GetBalancesTypedAsync(
        this ICoinPaymentProvider provider,
        CancellationToken cancellationToken = default)
    {
        var response = await provider.GetBalancesAsync(cancellationToken);
        return Deserialize<CoinPaymentsApiResponse<List<BalanceResult>>>(response);
    }

    // ── Currencies / Rates ────────────────────────────────────────────────

    /// <summary>Gets supported currencies and their rates.</summary>
    public static async Task<CoinPaymentsApiResponse<List<RateResult>>?> GetRatesAsync(
        this ICoinPaymentProvider provider,
        CancellationToken cancellationToken = default)
    {
        var response = await provider.GetCurrenciesAsync(cancellationToken);
        return Deserialize<CoinPaymentsApiResponse<List<RateResult>>>(response);
    }

    /// <summary>
    /// Gets all supported cryptocurrencies from the CoinPayments API v2 (v2/currencies endpoint).
    /// Endpoint público — no requiere autenticación.
    /// El API devuelve un array raíz JSON: [ { "id": "1", ... }, ... ]
    /// </summary>
    public static async Task<(bool IsSuccess, string Error, List<RateResult> Currencies)> GetCurrenciesTypedAsync(
        this ICoinPaymentProvider provider,
        CancellationToken cancellationToken = default)
    {
        var response = await provider.GetCurrenciesAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return (false, response.Content ?? "Unknown error from CoinPayments API", new List<RateResult>());

        if (string.IsNullOrWhiteSpace(response.Content))
            return (false, "Empty response from CoinPayments API", new List<RateResult>());

        try
        {
            var currencies = JsonSerializer.Deserialize<List<RateResult>>(response.Content, JsonOptions)
                             ?? new List<RateResult>();
            return (true, string.Empty, currencies);
        }
        catch (JsonException ex)
        {
            return (false, $"Failed to deserialize currencies: {ex.Message}", new List<RateResult>());
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static T? Deserialize<T>(RestResponse response) where T : class
    {
        if (string.IsNullOrWhiteSpace(response.Content))
            return null;
        try { return JsonSerializer.Deserialize<T>(response.Content, JsonOptions); }
        catch { return null; }
    }
}