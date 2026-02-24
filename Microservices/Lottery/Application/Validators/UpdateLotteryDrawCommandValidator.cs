using CryptoJackpot.Lottery.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class UpdateLotteryDrawCommandValidator : AbstractValidator<UpdateLotteryDrawCommand>
{
    public UpdateLotteryDrawCommandValidator()
    {
        RuleFor(c => c.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(c => c.MinNumber)
            .GreaterThanOrEqualTo(0).WithMessage("MinNumber must be greater than or equal to 0");

        RuleFor(c => c.MaxNumber)
            .GreaterThan(0).WithMessage("MaxNumber must be greater than 0")
            .GreaterThan(c => c.MinNumber).WithMessage("MaxNumber must be greater than MinNumber");
        
        RuleFor(c => c.TicketPrice)
            .GreaterThan(0).WithMessage("TicketPrice must be greater than 0");

        RuleFor(c => c.MaxTickets)
            .GreaterThan(0).WithMessage("MaxTickets must be greater than 0");

        RuleFor(c => c.StartDate)
            .NotEmpty().WithMessage("StartDate is required");

        RuleFor(c => c.EndDate)
            .NotEmpty().WithMessage("EndDate is required")
            .GreaterThan(c => c.StartDate).WithMessage("EndDate must be after StartDate");

        RuleFor(c => c.Status)
            .IsInEnum().WithMessage("Status must be a valid LotteryStatus");

        RuleFor(c => c.Type)
            .IsInEnum().WithMessage("Type must be a valid LotteryType");

        RuleFor(c => c.Terms)
            .NotEmpty().WithMessage("Terms is required");

        RuleFor(c => c.MinimumAge)
            .GreaterThanOrEqualTo(18).When(c => c.HasAgeRestriction)
            .WithMessage("MinimumAge must be at least 18 when age restriction is enabled");

        RuleFor(c => c.CryptoCurrencyId)
            .NotEmpty().WithMessage("CryptoCurrencyId is required")
            .MaximumLength(20).WithMessage("CryptoCurrencyId must not exceed 20 characters");

        RuleFor(c => c.CryptoCurrencySymbol)
            .NotEmpty().WithMessage("CryptoCurrencySymbol is required")
            .MaximumLength(20).WithMessage("CryptoCurrencySymbol must not exceed 20 characters");
    }
}

