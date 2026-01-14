using CryptoJackpot.Order.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Order.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("UserId is required");

        RuleFor(c => c.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");

        RuleFor(c => c.LotteryNumberIds)
            .NotEmpty().WithMessage("At least one lottery number must be selected");

        RuleFor(c => c.SelectedNumbers)
            .NotEmpty().WithMessage("SelectedNumbers is required");

        RuleFor(c => c.Series)
            .GreaterThan(0).WithMessage("Series must be greater than 0");

        RuleFor(c => c.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than 0");

        RuleFor(c => c.GiftRecipientId)
            .GreaterThan(0).When(c => c.IsGift)
            .WithMessage("GiftRecipientId is required when IsGift is true");
    }
}

