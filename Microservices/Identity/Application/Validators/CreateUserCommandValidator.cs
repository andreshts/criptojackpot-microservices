using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Identification)
            .MaximumLength(50).WithMessage("Identification must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Identification));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .Matches(@"^\+?[0-9\s\-\(\)]*$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be a valid phone number");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("RoleId must be greater than 0");

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("CountryId must be greater than 0");

        RuleFor(x => x.StatePlace)
            .NotEmpty().WithMessage("StatePlace is required")
            .MaximumLength(100).WithMessage("StatePlace must not exceed 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.Address)
            .MaximumLength(255).WithMessage("Address must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("ImagePath must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImagePath));

        RuleFor(x => x.ReferralCode)
            .MaximumLength(50).WithMessage("Referral code must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferralCode));
    }
}

