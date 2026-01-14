using CryptoJackpot.Lottery.Application.Queries;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class GetAllLotteryDrawsQueryValidator : AbstractValidator<GetAllLotteryDrawsQuery>
{
    public GetAllLotteryDrawsQueryValidator()
    {
        RuleFor(q => q.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50");
    }
}

