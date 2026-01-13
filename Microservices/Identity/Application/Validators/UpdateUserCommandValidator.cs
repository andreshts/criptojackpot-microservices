using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .Matches(@"^\+?[0-9\s\-\(\)]*$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be a valid phone number");

        RuleFor(x => x.Password)
            .MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must not exceed 100 characters")
            .Matches(@"[A-Z]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must contain at least one number");
    }
}

