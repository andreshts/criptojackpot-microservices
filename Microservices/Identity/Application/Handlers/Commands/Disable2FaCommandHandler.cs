using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class Disable2FaCommandHandler : IRequestHandler<Disable2FaCommand, Result>
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;

    public Disable2FaCommandHandler(ITwoFactorSetupService twoFactorSetupService)
    {
        _twoFactorSetupService = twoFactorSetupService;
    }

    public async Task<Result> Handle(Disable2FaCommand request, CancellationToken cancellationToken)
    {
        return await _twoFactorSetupService.DisableAsync(
            request.UserGuid,
            request.Code,
            request.RecoveryCode,
            cancellationToken);
    }
}

