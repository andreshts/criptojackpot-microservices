namespace CryptoJackpot.Wallet.Application.Requests;

/// <summary>
/// Request model for creating a cryptocurrency payment transaction
/// </summary>
public class CreateCoinPaymentTransactionRequest
{
    /// <summary>
    /// The amount in the source currency
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The source currency code (e.g., USD, EUR)
    /// </summary>
    public string CurrencyFrom { get; set; } = null!;

    /// <summary>
    /// The cryptocurrency code to receive (e.g., BTC, ETH, LTCT)
    /// </summary>
    public string CurrencyTo { get; set; } = null!;

    /// <summary>
    /// Optional buyer email for payment notifications
    /// </summary>
    public string? BuyerEmail { get; set; }

    /// <summary>
    /// Optional buyer name
    /// </summary>
    public string? BuyerName { get; set; }

    /// <summary>
    /// Optional description for the item being purchased
    /// </summary>
    public string? ItemName { get; set; }
}
