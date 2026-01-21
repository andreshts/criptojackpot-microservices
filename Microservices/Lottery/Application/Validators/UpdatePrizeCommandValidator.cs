using CryptoJackpot.Lottery.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class UpdatePrizeCommandValidator : AbstractValidator<UpdatePrizeCommand>
{
    public UpdatePrizeCommandValidator()
    {
        RuleFor(c => c.PrizeId)
            .NotEmpty().WithMessage("PrizeId is required");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(c => c.EstimatedValue)
            .GreaterThanOrEqualTo(0).WithMessage("EstimatedValue must be greater than or equal to 0");

        RuleFor(c => c.Type)
            .IsInEnum().WithMessage("Type must be a valid PrizeType");

        RuleFor(c => c.Tier)
            .GreaterThan(0).WithMessage("Tier must be greater than 0");

        RuleFor(c => c.MainImageUrl)
            .NotEmpty().WithMessage("MainImageUrl is required")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("MainImageUrl must be a valid absolute URL");

        RuleForEach(c => c.AdditionalImageUrls)
            .ChildRules(img =>
            {
                img.RuleFor(i => i.ImageUrl)
                    .NotEmpty().WithMessage("ImageUrl is required")
                    .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                    .WithMessage("ImageUrl must be a valid absolute URL");
                
                img.RuleFor(i => i.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be greater than or equal to 0");
            });

        RuleFor(c => c.CashAlternative)
            .GreaterThan(0).When(c => c.CashAlternative.HasValue)
            .WithMessage("CashAlternative must be greater than 0 when provided");
    }
}

