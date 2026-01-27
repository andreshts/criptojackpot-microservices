namespace CryptoJackpot.Wallet.Application.Responses;

/// <summary>
/// Response model for a created cryptocurrency payment transaction
/// </summary>
public class CreateCoinPaymentTransactionResponse
{
    /// <summary>
    /// The CoinPayments transaction ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// The cryptocurrency amount to be paid
    /// </summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// The cryptocurrency address for payment
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Number of confirmations required
    /// </summary>
    public string ConfirmsNeeded { get; set; } = string.Empty;

    /// <summary>
    /// Payment timeout in seconds
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// URL for the checkout page
    /// </summary>
    public string CheckoutUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL to check payment status
    /// </summary>
    public string StatusUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL for the QR code image
    /// </summary>
    public string QrCodeUrl { get; set; } = string.Empty;
}
