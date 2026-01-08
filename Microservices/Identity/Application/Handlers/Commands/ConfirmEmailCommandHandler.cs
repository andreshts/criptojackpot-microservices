using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;

    public ConfirmEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Fail<string>(new BadRequestError("Invalid confirmation token"));

        var user = await _userRepository.GetBySecurityCodeAsync(request.Token);

        if (user == null)
            return Result.Fail<string>(new NotFoundError("Invalid confirmation token"));

        if (user.Status)
            return Result.Fail<string>(new BadRequestError("Email already confirmed"));

        user.Status = true;
        user.SecurityCode = null;
        await _userRepository.UpdateAsync(user);

        return Result.Ok("Email confirmed successfully");
    }
}
