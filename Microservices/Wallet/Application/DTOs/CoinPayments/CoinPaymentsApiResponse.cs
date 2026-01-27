using System.Text.Json.Serialization;

namespace CryptoJackpot.Wallet.Application.DTOs.CoinPayments;

/// <summary>
/// Base response from CoinPayments API
/// </summary>
/// <typeparam name="T">Type of the result payload</typeparam>
public class CoinPaymentsApiResponse<T> where T : class
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    public bool IsSuccess => Error == "ok" && Result != null;
}

/// <summary>
/// Response for get_callback_address command
/// </summary>
public class GetCallbackAddressResult
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("dest_tag")]
    public string? DestTag { get; set; }
}

/// <summary>
/// Response for create_transaction command
/// </summary>
public class CreateTransactionResult
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("txn_id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("confirms_needed")]
    public string ConfirmsNeeded { get; set; } = string.Empty;

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; }

    [JsonPropertyName("checkout_url")]
    public string CheckoutUrl { get; set; } = string.Empty;

    [JsonPropertyName("status_url")]
    public string StatusUrl { get; set; } = string.Empty;

    [JsonPropertyName("qrcode_url")]
    public string QrCodeUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response for get_tx_info command
/// </summary>
public class TransactionInfoResult
{
    [JsonPropertyName("time_created")]
    public long TimeCreated { get; set; }

    [JsonPropertyName("time_expires")]
    public long TimeExpires { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("status_text")]
    public string StatusText { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("amountf")]
    public string AmountFormatted { get; set; } = string.Empty;

    [JsonPropertyName("received")]
    public string Received { get; set; } = string.Empty;

    [JsonPropertyName("receivedf")]
    public string ReceivedFormatted { get; set; } = string.Empty;

    [JsonPropertyName("recv_confirms")]
    public int ReceivedConfirms { get; set; }

    [JsonPropertyName("payment_address")]
    public string PaymentAddress { get; set; } = string.Empty;
}

/// <summary>
/// Response for balances command
/// </summary>
public class BalanceResult
{
    [JsonPropertyName("balance")]
    public long Balance { get; set; }

    [JsonPropertyName("balancef")]
    public string BalanceFormatted { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("coin_status")]
    public string CoinStatus { get; set; } = string.Empty;
}

/// <summary>
/// Response for rates command
/// </summary>
public class RateResult
{
    [JsonPropertyName("is_fiat")]
    public int IsFiat { get; set; }

    [JsonPropertyName("rate_btc")]
    public string RateBtc { get; set; } = string.Empty;

    [JsonPropertyName("last_update")]
    public string LastUpdate { get; set; } = string.Empty;

    [JsonPropertyName("tx_fee")]
    public string TransactionFee { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("confirms")]
    public string Confirms { get; set; } = string.Empty;

    [JsonPropertyName("accepted")]
    public int Accepted { get; set; }
}
