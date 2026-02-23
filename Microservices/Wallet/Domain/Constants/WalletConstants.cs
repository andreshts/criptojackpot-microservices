using System.Diagnostics.CodeAnalysis;

namespace CryptoJackpot.Wallet.Domain.Constants;

/// <summary>
/// Configuration keys for the Wallet microservice
/// </summary>
public static class ConfigurationKeys
{
    public const string CoinPaymentsSection = "CoinPayments";
    public const string CoinPaymentsClientSecret = "CoinPayments:ClientSecret";
    public const string CoinPaymentsClientId = "CoinPayments:ClientId";
    public const string CoinPaymentsBaseUrl = "CoinPayments:BaseUrl";
}

/// <summary>
/// Default values for external services
/// </summary>
[SuppressMessage("Design", "S1075:URIs should not be hardcoded",
    Justification = "Default fallback URL for CoinPayments API, can be overridden via configuration")]
public static class ServiceDefaults
{
    /// <summary>
    /// Default CoinPayments API endpoint URL
    /// </summary>
    public const string CoinPaymentsBaseUrl = "https://a-api.coinpayments.net/api/";

    /// <summary>
    /// Default HTTP client timeout in seconds
    /// </summary>
    public const int HttpClientTimeoutSeconds = 30;

    /// <summary>
    /// Named HttpClient for CoinPayments
    /// </summary>
    public const string CoinPaymentsHttpClient = "CoinPayments";
}

public static class CoinPaymentsEndpoints
{
    // ── Invoices (v2) ─────────────────────────────────────────────────
    public const string CreateInvoice = "v2/merchant/invoices";
    public const string GetInvoices = "v2/merchant/invoices";
    public const string GetInvoiceById = "v2/merchant/invoices/{0}";
    public const string GetInvoicePayouts = "v2/merchant/invoices/{0}/payouts";
    public const string GetInvoiceHistory = "v2/merchant/invoices/{0}/history";

    // ── Invoices (v1 — mixed versioning per CoinPayments docs) ────────
    public const string CancelInvoice = "v1/merchant/invoices/cancel/{0}";
    public const string GetInvoicePaymentAddress = "v1/invoices/{0}/payment-currencies/{1}";
    public const string GetInvoicePaymentStatus = "v1/invoices/{0}/payment-currencies/{1}/status";
    public const string CreateInvoicePayment = "v1/invoices/{0}/payments";

    // ── Wallets / Balances ────────────────────────────────────────────
    public const string GetBalances = "v1/merchant/wallets";

    // ── Currencies / Rates ────────────────────────────────────────────
    public const string GetCurrencies = "v1/currencies";
    public const string GetRates = "v1/rates";

    // ── Withdrawals ───────────────────────────────────────────────────
    public const string CreateWithdrawal = "v1/merchant/withdrawals";
}

/// <summary>
/// Resilience policy settings
/// </summary>
public static class ResilienceSettings
{
    /// <summary>
    /// Number of retry attempts for transient errors
    /// </summary>
    public const int RetryCount = 3;

    /// <summary>
    /// Number of consecutive failures before circuit breaker opens
    /// </summary>
    public const int CircuitBreakerFailureThreshold = 5;

    /// <summary>
    /// Duration in seconds the circuit breaker stays open
    /// </summary>
    public const int CircuitBreakerDurationSeconds = 30;
}