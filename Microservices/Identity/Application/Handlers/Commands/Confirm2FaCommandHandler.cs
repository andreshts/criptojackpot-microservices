using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class Confirm2FaCommandHandler : IRequestHandler<Confirm2FaCommand, Result<Confirm2FaResultDto>>
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;

    public Confirm2FaCommandHandler(ITwoFactorSetupService twoFactorSetupService)
    {
        _twoFactorSetupService = twoFactorSetupService;
    }

    public async Task<Result<Confirm2FaResultDto>> Handle(
        Confirm2FaCommand request, 
        CancellationToken cancellationToken)
    {
        return await _twoFactorSetupService.ConfirmSetupAsync(
            request.UserGuid, 
            request.Code, 
            cancellationToken);
    }
}

