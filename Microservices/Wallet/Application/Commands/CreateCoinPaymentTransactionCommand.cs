using CryptoJackpot.Wallet.Application.Responses;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

/// <summary>
/// Command to create a cryptocurrency payment transaction via CoinPayments
/// </summary>
public class CreateCoinPaymentTransactionCommand : IRequest<Result<CreateCoinPaymentTransactionResponse>>
{
    /// <summary>
    /// The amount in the source currency
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The source currency (e.g., USD, EUR)
    /// </summary>
    public string CurrencyFrom { get; set; } = null!;

    /// <summary>
    /// The cryptocurrency to receive (e.g., BTC, ETH, LTCT)
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
    /// Optional description/name for the item being purchased
    /// </summary>
    public string? ItemName { get; set; }

    /// <summary>
    /// Optional IPN (Instant Payment Notification) callback URL
    /// </summary>
    public string? IpnUrl { get; set; }
}
