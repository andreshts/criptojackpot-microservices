using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class LogoutAllDevicesCommandValidator : AbstractValidator<LogoutAllDevicesCommand>
{
    public LogoutAllDevicesCommandValidator()
    {
        RuleFor(x => x.UserGuid)
            .NotEmpty()
            .WithMessage("User identifier is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .WithMessage("Reason cannot exceed 200 characters.");
    }
}

