using CryptoJackpot.Lottery.Application.Queries;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class GetLotteryDrawByIdQueryValidator : AbstractValidator<GetLotteryDrawByIdQuery>
{
    public GetLotteryDrawByIdQueryValidator()
    {
        RuleFor(q => q.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");
    }
}

