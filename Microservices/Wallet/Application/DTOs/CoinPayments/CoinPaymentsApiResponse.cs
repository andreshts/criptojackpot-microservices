using System.Text.Json.Serialization;

namespace CryptoJackpot.Wallet.Application.DTOs.CoinPayments;

// ─────────────────────────────────────────────
// Base wrapper for all new API v2 responses
// ─────────────────────────────────────────────

/// <summary>
/// Generic response envelope from the new CoinPayments API v2.
/// </summary>
public class CoinPaymentsApiResponse<T> where T : class
{
    [JsonPropertyName("invoice")]
    public T? Result { get; init; }

    // Used by list/single endpoints that return directly
    [JsonPropertyName("items")]
    public List<T>? Items { get; init; }

    /// <summary>HTTP-level success; set by the provider after reading the HTTP status code.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Error message populated on failure.</summary>
    public string Error { get; init; } = string.Empty;
}

// ─────────────────────────────────────────────
// Invoice / Transaction (POST /merchant/invoices)
// ─────────────────────────────────────────────

/// <summary>Request body to create a new merchant invoice.</summary>
public class CreateInvoiceRequest
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public InvoiceAmount Amount { get; set; } = new();

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("displayValue")]
    public string? DisplayValue { get; set; }

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

public class InvoiceAmount
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

/// <summary>Invoice result from the new CoinPayments API v2.</summary>
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
    public InvoiceAmount? Amount { get; set; }

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

// ─────────────────────────────────────────────
// Transaction / Invoice info (GET /merchant/invoices/{id})
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
    public InvoiceAmount? Amount { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;

    [JsonPropertyName("completedAt")]
    public string? CompletedAt { get; set; }
}

// ─────────────────────────────────────────────
// Balances (GET /merchant/balance)
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
// Currencies / Rates (GET /currencies)
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
// Withdrawal (POST /merchant/withdrawals)
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

// ─────────────────────────────────────────────
// Kept for backward compat – no longer used
// ─────────────────────────────────────────────

/// <summary>Legacy callback-address result stub (not used by new API).</summary>
public class GetCallbackAddressResult
{
    public string Address { get; set; } = string.Empty;
    public string? DestTag { get; set; }
}
