using CryptoJackpot.Lottery.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class DeleteLotteryDrawCommandValidator : AbstractValidator<DeleteLotteryDrawCommand>
{
    public DeleteLotteryDrawCommandValidator()
    {
        RuleFor(c => c.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");
    }
}

