using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class Setup2FaCommandValidator : AbstractValidator<Setup2FaCommand>
{
    public Setup2FaCommandValidator()
    {
        RuleFor(x => x.UserGuid)
            .NotEmpty()
            .WithMessage("User identifier is required.");
    }
}

