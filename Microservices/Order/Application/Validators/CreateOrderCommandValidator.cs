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

        RuleFor(c => c.Items)
            .NotEmpty().WithMessage("At least one item must be selected");

        RuleForEach(c => c.Items).SetValidator(new CreateOrderItemCommandValidator());
    }
}

public class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
{
    public CreateOrderItemCommandValidator()
    {
        RuleFor(i => i.Number)
            .GreaterThan(0).WithMessage("Number must be greater than 0");

        RuleFor(i => i.Series)
            .GreaterThan(0).WithMessage("Series must be greater than 0");

        RuleFor(i => i.UnitPrice)
            .GreaterThan(0).WithMessage("UnitPrice must be greater than 0");

        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(i => i.GiftRecipientId)
            .GreaterThan(0).When(i => i.IsGift)
            .WithMessage("GiftRecipientId is required when IsGift is true");
    }
}

