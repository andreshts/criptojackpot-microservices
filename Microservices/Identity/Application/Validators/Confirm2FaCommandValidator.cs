using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class Confirm2FaCommandValidator : AbstractValidator<Confirm2FaCommand>
{
    public Confirm2FaCommandValidator()
    {
        RuleFor(x => x.UserGuid)
            .NotEmpty()
            .WithMessage("User identifier is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("TOTP code is required.")
            .Length(6)
            .WithMessage("TOTP code must be 6 digits.")
            .Matches(@"^\d{6}$")
            .WithMessage("TOTP code must contain only digits.");
    }
}

