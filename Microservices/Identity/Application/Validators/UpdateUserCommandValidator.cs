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

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("CountryId must be greater than 0");

        RuleFor(x => x.StatePlace)
            .NotEmpty().WithMessage("StatePlace is required")
            .MaximumLength(100).WithMessage("StatePlace must not exceed 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.Address)
            .MaximumLength(150).WithMessage("Address must not exceed 150 characters");
    }
}

