using CryptoJackpot.Lottery.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class DeletePrizeCommandValidator : AbstractValidator<DeletePrizeCommand>
{
    public DeletePrizeCommandValidator()
    {
        RuleFor(c => c.PrizeId)
            .NotEmpty().WithMessage("PrizeId is required");
    }
}

