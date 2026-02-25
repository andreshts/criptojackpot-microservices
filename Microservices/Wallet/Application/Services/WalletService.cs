﻿using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Extensions;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using FluentResults;

namespace CryptoJackpot.Wallet.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletBalanceRepository _balanceRepository;
    private readonly IUnitOfWork _uow;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletBalanceRepository balanceRepository,
        IUnitOfWork uow)
    {
        _walletRepository = walletRepository;
        _balanceRepository = balanceRepository;
        _uow = uow;
    }

    public async Task<Result<WalletTransaction>> ApplyTransactionAsync(
        Guid userGuid,
        decimal amount,
        WalletTransactionDirection direction,
        WalletTransactionType type,
        Guid? referenceId = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result.Fail(new BadRequestError("Transaction amount must be greater than zero."));

        if (!type.IsCoherentWith(direction))
            return Result.Fail(new BadRequestError($"Transaction type '{type}' is not valid for a {direction} movement."));

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get or create balance row
            var balance = await _balanceRepository.GetByUserAsync(userGuid, cancellationToken);
            var isNewBalance = balance is null;
            balance ??= await _balanceRepository.AddAsync(new WalletBalance { UserGuid = userGuid }, cancellationToken);

            // Guard: insufficient funds for debits
            if (direction == WalletTransactionDirection.Debit && balance.Balance < amount)
                return Result.Fail(new BadRequestError("Insufficient balance."));

            // Apply movement to balance
            if (direction == WalletTransactionDirection.Credit)
            {
                balance.Balance += amount;
                balance.TotalEarned += amount;
            }
            else
            {
                balance.Balance -= amount;

                if (type is WalletTransactionType.Withdrawal or WalletTransactionType.WithdrawalRefund)
                    balance.TotalWithdrawn += amount;
                else
                    balance.TotalPurchased += amount;
            }

            // Rotate concurrency token
            balance.RowVersion = Guid.NewGuid();
            balance.UpdatedAt = DateTime.UtcNow;

            // Only call Update for existing entities — newly added entities are already tracked
            if (!isNewBalance)
                _balanceRepository.Update(balance);

            // Record transaction with balance snapshot
            var transaction = await _walletRepository.AddAsync(new WalletTransaction
            {
                UserGuid     = userGuid,
                Amount       = amount,
                Direction    = direction,
                Type         = type,
                Status       = WalletTransactionStatus.Completed,
                BalanceAfter = balance.Balance,
                ReferenceId  = referenceId,
                Description  = description,
            }, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);

            return Result.Ok(transaction);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
