using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class Setup2FaCommandHandler : IRequestHandler<Setup2FaCommand, Result<Setup2FaResultDto>>
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;

    public Setup2FaCommandHandler(ITwoFactorSetupService twoFactorSetupService)
    {
        _twoFactorSetupService = twoFactorSetupService;
    }

    public async Task<Result<Setup2FaResultDto>> Handle(
        Setup2FaCommand request, 
        CancellationToken cancellationToken)
    {
        return await _twoFactorSetupService.InitiateSetupAsync(request.UserGuid, cancellationToken);
    }
}

