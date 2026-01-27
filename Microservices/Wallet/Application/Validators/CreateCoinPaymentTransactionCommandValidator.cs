using CryptoJackpot.Wallet.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Wallet.Application.Validators;

public class CreateCoinPaymentTransactionCommandValidator : AbstractValidator<CreateCoinPaymentTransactionCommand>
{
    private static readonly string[] SupportedFiatCurrencies = ["USD", "EUR", "GBP", "CAD", "AUD", "JPY"];
    private static readonly string[] SupportedCryptoCurrencies = ["BTC", "ETH", "LTC", "LTCT", "USDT", "USDC", "DOGE", "XRP"];

    public CreateCoinPaymentTransactionCommandValidator()
    {
        RuleFor(c => c.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount must not exceed 1,000,000");

        RuleFor(c => c.CurrencyFrom)
            .NotEmpty().WithMessage("CurrencyFrom is required")
            .Length(2, 10).WithMessage("CurrencyFrom must be between 2 and 10 characters")
            .Must(BeValidFiatCurrency).WithMessage($"CurrencyFrom must be one of: {string.Join(", ", SupportedFiatCurrencies)}");

        RuleFor(c => c.CurrencyTo)
            .NotEmpty().WithMessage("CurrencyTo is required")
            .Length(2, 10).WithMessage("CurrencyTo must be between 2 and 10 characters")
            .Must(BeValidCryptoCurrency).WithMessage($"CurrencyTo must be one of: {string.Join(", ", SupportedCryptoCurrencies)}");

        RuleFor(c => c.BuyerEmail)
            .EmailAddress().WithMessage("BuyerEmail must be a valid email address")
            .MaximumLength(255).WithMessage("BuyerEmail must not exceed 255 characters")
            .When(c => !string.IsNullOrEmpty(c.BuyerEmail));

        RuleFor(c => c.BuyerName)
            .MaximumLength(100).WithMessage("BuyerName must not exceed 100 characters")
            .When(c => !string.IsNullOrEmpty(c.BuyerName));

        RuleFor(c => c.ItemName)
            .MaximumLength(255).WithMessage("ItemName must not exceed 255 characters")
            .When(c => !string.IsNullOrEmpty(c.ItemName));

        RuleFor(c => c.IpnUrl)
            .Must(BeValidUrl).WithMessage("IpnUrl must be a valid URL")
            .MaximumLength(500).WithMessage("IpnUrl must not exceed 500 characters")
            .When(c => !string.IsNullOrEmpty(c.IpnUrl));
    }

    private static bool BeValidFiatCurrency(string currency)
    {
        return SupportedFiatCurrencies.Contains(currency.ToUpperInvariant());
    }

    private static bool BeValidCryptoCurrency(string currency)
    {
        return SupportedCryptoCurrencies.Contains(currency.ToUpperInvariant());
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
