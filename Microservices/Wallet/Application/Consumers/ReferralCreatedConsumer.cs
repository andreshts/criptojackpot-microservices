using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Consumers;

/// <summary>
/// Consumes <see cref="ReferralCreatedEvent"/> from Identity service via Kafka.
/// Credits $5.00 USD referral bonus to the referrer's internal wallet.
/// </summary>
public class ReferralCreatedConsumer : IConsumer<ReferralCreatedEvent>
{
    private const decimal ReferralBonusAmount = 5.00m;

    private readonly IWalletService _walletService;
    private readonly ILogger<ReferralCreatedConsumer> _logger;

    public ReferralCreatedConsumer(IWalletService walletService, ILogger<ReferralCreatedConsumer> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received ReferralCreatedEvent — Referrer: {ReferrerGuid}, Referred: {ReferredGuid}, Code: {Code}",
            message.ReferrerUserGuid, message.ReferredUserGuid, message.ReferralCode);

        var description = $"Referral bonus — {message.ReferredName} {message.ReferredLastName} joined with code {message.ReferralCode}";

        var result = await _walletService.ApplyTransactionAsync(
            userGuid: message.ReferrerUserGuid,
            amount: ReferralBonusAmount,
            direction: WalletTransactionDirection.Credit,
            type: WalletTransactionType.ReferralBonus,
            referenceId: null,
            description: description,
            cancellationToken: context.CancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Referral bonus of {Amount} USD credited to referrer {ReferrerGuid}. Transaction: {TxGuid}",
                ReferralBonusAmount, message.ReferrerUserGuid, result.Value.TransactionGuid);
        }
        else
        {
            _logger.LogError(
                "Failed to credit referral bonus to referrer {ReferrerGuid}. Errors: {Errors}",
                message.ReferrerUserGuid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }
    }
}

