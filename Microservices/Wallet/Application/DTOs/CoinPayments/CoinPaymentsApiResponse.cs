using System.Text.Json.Serialization;

namespace CryptoJackpot.Wallet.Application.DTOs.CoinPayments;

// ─────────────────────────────────────────────
// Base wrapper for all API v2 responses
// ─────────────────────────────────────────────

/// <summary>
/// Generic response envelope from the CoinPayments API v2.
/// The API returns different root keys depending on the endpoint:
///   POST /merchant/invoices  → { "invoices": [ ... ] }
///   GET  /merchant/invoices  → { "invoices": [ ... ] }
///   GET  /merchant/invoices/{id} → { "invoices": [ ... ] }  (single-element array)
/// </summary>
public class CoinPaymentsApiResponse<T> where T : class
{
    /// <summary>
    /// Used by invoice endpoints — returns an array even for single-creation.
    /// Ref: PHP official example → $response['invoices']
    /// </summary>
    [JsonPropertyName("invoices")]
    public List<T>? Invoices { get; init; }

    /// <summary>
    /// Used by list/paginated endpoints that return "items".
    /// </summary>
    [JsonPropertyName("items")]
    public List<T>? Items { get; init; }

    /// <summary>HTTP-level success; set by the provider after reading the HTTP status code.</summary>
    [JsonIgnore]
    public bool IsSuccess { get; set; }

    /// <summary>Error message populated on failure.</summary>
    [JsonIgnore]
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Convenience accessor: returns the first invoice or default from the response.
    /// </summary>
    [JsonIgnore]
    public T? FirstResult => Invoices?.FirstOrDefault() ?? Items?.FirstOrDefault();
}

// ─────────────────────────────────────────────
// Invoice / Transaction (POST /api/v2/merchant/invoices)
// ─────────────────────────────────────────────

/// <summary>
/// Request body to create a new merchant invoice (API v2).
/// Ref: Official C# example — CoinPaymentsCom/Examples (cs_invoice_v2 branch).
/// Currency values must be numeric IDs (e.g., "5057" for USD, "1002" for LTCT),
/// NOT ticker symbols.
/// </summary>
public class CreateInvoiceRequest
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Invoice currency ID (numeric). Example: "5057" (USD), "1002" (LTCT).
    /// This is the denomination currency shown on the invoice.
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Line items on the invoice. At minimum one item is required.
    /// </summary>
    [JsonPropertyName("items")]
    public List<InvoiceItem> Items { get; set; } = new();

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("notesLink")]
    public string? NotesLink { get; set; }

    [JsonPropertyName("requireBuyerNameAndEmail")]
    public bool? RequireBuyerNameAndEmail { get; set; }

    [JsonPropertyName("buyerDataCollectionMessage")]
    public string? BuyerDataCollectionMessage { get; set; }

    [JsonPropertyName("customData")]
    public string? CustomData { get; set; }

    [JsonPropertyName("webhookData")]
    public InvoiceWebhookData? WebhookData { get; set; }
}

/// <summary>
/// Individual line item within an invoice.
/// </summary>
public class InvoiceItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public InvoiceItemQuantity Quantity { get; set; } = new();

    /// <summary>
    /// Amount as string (e.g., "10.50"). 
    /// For the "originalAmount" variant, the value is in the invoice's currency.
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Original amount breakdown if needed (optional).
    /// </summary>
    [JsonPropertyName("originalAmount")]
    public InvoiceOriginalAmount? OriginalAmount { get; set; }
}

public class InvoiceItemQuantity
{
    /// <summary>Quantity value (integer or decimal).</summary>
    [JsonPropertyName("value")]
    public int Value { get; set; } = 1;

    /// <summary>Quantity type. 2 = integer quantity.</summary>
    [JsonPropertyName("type")]
    public int Type { get; set; } = 2;
}

public class InvoiceOriginalAmount
{
    [JsonPropertyName("currencyId")]
    public string CurrencyId { get; set; } = string.Empty;

    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class InvoiceWebhookData
{
    [JsonPropertyName("notificationsUrl")]
    public string NotificationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public Dictionary<string, string>? Params { get; set; }
}

// ─────────────────────────────────────────────
// Invoice creation response
// ─────────────────────────────────────────────

/// <summary>
/// Single invoice result returned inside the "invoices" array.
/// </summary>
public class CreateTransactionResult
{
    [JsonPropertyName("id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("statusUrl")]
    public string StatusUrl { get; set; } = string.Empty;

    [JsonPropertyName("checkoutUrl")]
    public string CheckoutUrl { get; set; } = string.Empty;

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public InvoiceAmountResponse? Amount { get; set; }

    [JsonPropertyName("payment")]
    public InvoicePayment? Payment { get; set; }

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;

    // Convenience aliases kept for backward compat with existing mapping profile
    [JsonIgnore]
    public string Address => string.Empty;

    [JsonIgnore]
    public string ConfirmsNeeded => string.Empty;

    [JsonIgnore]
    public int Timeout => 0;
}

/// <summary>Amount as returned in invoice responses.</summary>
public class InvoiceAmountResponse
{
    [JsonPropertyName("currencyId")]
    public string CurrencyId { get; set; } = string.Empty;

    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>Payment info returned on invoice creation (contains paymentCurrencies array).</summary>
public class InvoicePayment
{
    [JsonPropertyName("paymentCurrencies")]
    public List<InvoicePaymentCurrency>? PaymentCurrencies { get; set; }
}

public class InvoicePaymentCurrency
{
    [JsonPropertyName("currency")]
    public PaymentCurrencyInfo? Currency { get; set; }

    [JsonPropertyName("remainingAmount")]
    public InvoiceAmountResponse? RemainingAmount { get; set; }
}

public class PaymentCurrencyInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────
// Transaction / Invoice info (GET /api/v2/merchant/invoices/{id})
// ─────────────────────────────────────────────

public class TransactionInfoResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("statusUrl")]
    public string StatusUrl { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public InvoiceAmountResponse? Amount { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;

    [JsonPropertyName("completedAt")]
    public string? CompletedAt { get; set; }
}

// ─────────────────────────────────────────────
// Balances (GET /api/v1/merchant/wallets)
// ─────────────────────────────────────────────

public class BalanceResult
{
    [JsonPropertyName("currencyId")]
    public string CurrencyId { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("balance")]
    public string Balance { get; set; } = string.Empty;

    [JsonPropertyName("balanceFormatted")]
    public string BalanceFormatted { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────
// Currencies / Rates (GET /api/v1/currencies)
// ─────────────────────────────────────────────

public class RateResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("decimalPlaces")]
    public int DecimalPlaces { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("isSettlement")]
    public bool IsSettlement { get; set; }

    [JsonPropertyName("isFiat")]
    public bool IsFiat { get; set; }

    [JsonPropertyName("rateUsd")]
    public string RateUsd { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────
// Withdrawal (POST /api/v1/merchant/withdrawals)
// ─────────────────────────────────────────────

public class CreateWithdrawalRequest
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("autoConfirm")]
    public bool AutoConfirm { get; set; }

    [JsonPropertyName("notificationsUrl")]
    public string? NotificationsUrl { get; set; }
}