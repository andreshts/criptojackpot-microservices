using CryptoJackpot.Domain.Core.Responses;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface ICoinPaymentProvider
{
    /// <summary>Creates a merchant invoice (transaction) to receive payment.</summary>
    Task<RestResponse> CreateInvoiceAsync(
        decimal amount,
        string currencyFrom,
        string currencyTo,
        string? notes = null,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets information about a specific invoice/transaction by ID.</summary>
    Task<RestResponse> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Gets merchant wallet balances.</summary>
    Task<RestResponse> GetBalancesAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets supported currencies and exchange rates.</summary>
    Task<RestResponse> GetCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a withdrawal/payout to an external address.</summary>
    Task<RestResponse> CreateWithdrawalAsync(
        decimal amount,
        string currency,
        string address,
        bool autoConfirm = false,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default);
}