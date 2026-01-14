using CryptoJackpot.Order.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Order.Application.Validators;

public class CompleteOrderCommandValidator : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderCommandValidator()
    {
        RuleFor(c => c.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("UserId is required");

        RuleFor(c => c.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required")
            .MaximumLength(100).WithMessage("TransactionId must not exceed 100 characters");
    }
}

